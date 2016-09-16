using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Player;
using RazzleServer.Packet;
using RazzleServer.Inventory;
using RazzleServer.Data;
using RazzleServer.Util;
using RazzleServer.Constants;
using RazzleServer.Server;
using RazzleServer.Data.WZ;
using System.Drawing;

namespace RazzleServer.Map.Monster
{
    public class MapleMonster
    {
        public int HP { get; private set; }
        public bool Alive { get; set; }

        public WeakReference<MapleCharacter> Controller { get; set; }
        private Dictionary<int, long> AttackerDamageList = new Dictionary<int, long>();
        public object MobLock = new object();

        public int ObjectID { get; set; }
        public Point Position { get; set; }
        public short Fh { get; set; }
        public byte Stance { get; set; }
        public bool IsBoss { get { return WzInfo.IsBoss; } }
        public WzMob WzInfo { get; private set; }
        public MapleMap Map { get; private set; }

        private List<MonsterBuff> Buffs = new List<MonsterBuff>();
        private Dictionary<int, List<int>> stolenItems = new Dictionary<int, List<int>>();


        public bool ControllerHasAggro { get; set; }
        public Dictionary<MobSkill, DateTime> SkillTimes = new Dictionary<MobSkill, DateTime>();

        public MapleMonster(WzMob Info, MapleMap map)
        {
            WzInfo = Info;
            Controller = new WeakReference<MapleCharacter>(null);
            ControllerHasAggro = false;
            HP = WzInfo.HP;
            Stance = 2;
            Alive = true;
            Map = map;
            foreach (MobSkill skill in Info.Skills)
            {
                SkillTimes[skill] = DateTime.MinValue;
            }
        }

        public void Damage(MapleCharacter from, int damage, bool showHp = true, bool fromPoison = false)
        {
            lock (MobLock)
            {
                if (!Alive) //already dead
                    return;

                if (!ControllerHasAggro && !from.Hidden)
                {
                    SetController(from, ObjectID, true);
                }

                if (fromPoison && damage >= HP)
                {
                    damage = HP - 1;
                    if (damage < 1)
                        return;
                }
                HP -= damage;
                bool kill = false;

                if (HP <= 0)
                {
                    kill = true;
                    damage -= (int)Math.Abs(HP);
                    HP = 0;
                }
                if (from != null)
                {
                    AddAttackerDamage(from.ID, damage);
                    if (showHp)
                        from.Client.SendPacket(UpdateHp(ObjectID, HpPercent));
                }

                if (kill)
                {
                    Kill(from);
                }
            }
        }

        public void ApplyPoison(int sourceSkillId, int durationMS, int damage, int intervalMS, MapleCharacter applicant, int maxStacks = 1)
        {
            lock (Buffs)
            {
                int currentStacks = Buffs.Where(x => x.SkillId == sourceSkillId && x.OwnerId == applicant.ID).Count();
                if (currentStacks < maxStacks)
                {
                    // TODO: POISON
                    //Poison poison = new Poison(sourceSkillId, durationMS, this, damage, intervalMS, applicant);
                    //Buffs.Add(poison);
                    //Map.BroadcastPacket(poison.GetApplicationPacket());
                }
            }
        }

        public void ApplyStatusEffect(int sourceSkillId, BuffStat buffStat, int buffValue, int durationMS, MapleCharacter applicant)
        {
            lock (Buffs)
            {
                List<MonsterBuff> toRemove = new List<MonsterBuff>();
                foreach (MonsterBuff buff in Buffs)
                {
                    if (buff.BuffStat == buffStat) //Mob already has this buff
                    {
                        toRemove.Add(buff);
                    }
                }
                foreach (var buff in toRemove)
                    buff.Dispose(true);
                MonsterBuff newEffect = new MonsterBuff(applicant.ID, sourceSkillId, durationMS, buffStat, buffValue, this);
                Buffs.Add(newEffect);
                Map.BroadcastPacket(newEffect.GetApplicationPacket());
            }
        }

        public void RemoveStatusEffect(MonsterBuff effect)
        {
            lock (Buffs)
            {
                Buffs.Remove(effect);
            }
        }

        public void Kill(MapleCharacter killer, bool dropItems = true)
        {
            lock (MobLock)
            {
                Alive = false;
                int exp = WzInfo.Exp * ServerConfig.Instance.ExpRate;
                MapleCharacter dropOwner = killer;
                int HighestExp = 0;
                foreach (KeyValuePair<int, long> kvp in AttackerDamageList)
                {
                    MapleCharacter attacker = Map.GetCharacter(kvp.Key);
                    if (attacker != null)
                    {
                        double expPercent = ((double)Math.Min(kvp.Value, WzInfo.HP) / (double)WzInfo.HP);
                        int expToGive = (int)((expPercent * exp) * (attacker.Stats.ExpR / 100.0));
                        //Drops go to the person who did the most damage
                        if (expToGive > HighestExp)
                        {
                            HighestExp = expToGive;
                            dropOwner = attacker;
                        }
                        attacker.GainExp(expToGive, true, true);
                    }
                }
                if (dropItems)
                {
                    Map.SpawnMapItemsFromMonster(this, Position, dropOwner);
                }
                dropOwner.UpdateQuestKills(WzInfo.MobId); //TODO: whole party on map
                Map.BroadcastPacket(KillMob(ObjectID));
                Map.RemoveMob(ObjectID);

                #region Final Pact
                if (dropOwner.Job == JobConstants.DARKKNIGHT)
                {
                    Buff buff = dropOwner.GetBuff(DarkKnight.FINAL_PACT2);
                    if (buff != null)
                    {
                        buff.Stacks--;
                        if (buff.Stacks <= 0)
                        {
                            dropOwner.Client.SendPacket(Skill.ShowBuffEffect(DarkKnight.FINAL_PACT2, dropOwner.Level, null, false));
                            dropOwner.CancelBuff(DarkKnight.FINAL_PACT2);
                        }
                        else
                        {
                            uint remainingTimeMS = (uint)buff.Duration - (uint)DateTime.UtcNow.Subtract(buff.StartTime).TotalMilliseconds;
                            dropOwner.Client.SendPacket(Buff.UpdateFinalPactKillCount(buff.Stacks, remainingTimeMS));
                        }
                    }
                }
                #endregion

                Map = null;
                foreach (MonsterBuff effect in Buffs)
                {
                    effect.Dispose(true);
                }
            }
        }

        #region helpers
        public byte HpPercent
        {
            get
            {
                byte percent = (byte)Math.Round(((double)HP / (double)WzInfo.HP) * 100.0);
                percent = Math.Min(percent, (byte)100);
                return percent;
            }
        }

        private void AddAttackerDamage(int characterId, long damage)
        {
            if (AttackerDamageList.ContainsKey(characterId))
            {
                AttackerDamageList[characterId] += damage;
            }
            else
            {
                AttackerDamageList.Add(characterId, damage);
            }
        }

        #region Bandit Steal
        public bool PlayerHasStolenItem(int characterId, int itemId)
        {
            List<int> stolen;
            if (stolenItems.TryGetValue(characterId, out stolen))
            {
                return stolen.Contains(itemId);
            }
            return false;
        }

        public void AddStolenItem(int characterId, int itemId)
        {
            List<int> stolen;
            if (stolenItems.TryGetValue(characterId, out stolen))
                stolen.Add(itemId);
            else
                stolenItems.Add(characterId, new List<int>() { itemId });
        }

        public MapleItem TryGetStealableItem(int characterId, string characterName)
        {
            List<int> alreadyStolenItems;
            if (!stolenItems.TryGetValue(characterId, out alreadyStolenItems))
                alreadyStolenItems = new List<int>();
            int potionId = IsBoss ? 2431835 : 2431835; //TODO: correct potion id
            string source = string.Format("Steal skill from mob {0} by player {1}", WzInfo.MobId, characterName);
            if (!alreadyStolenItems.Contains(potionId))
            {
                MapleItem item = new MapleItem(potionId, source);
                AddStolenItem(characterId, potionId);
                return item;
            }
            if (!IsBoss)
            {
                List<MobDrop> drops = DataBuffer.GetMobDropsById(WzInfo.MobId);
                foreach (MobDrop mobDrop in drops)
                {
                    if (mobDrop.QuestID > 0 || alreadyStolenItems.Contains(mobDrop.ItemID))
                        continue;
                    int chance = (int)(mobDrop.DropChance);
                    if (Functions.Random(0, 999999) <= chance)
                    {
                        int amount = Functions.Random(mobDrop.MinQuantity, mobDrop.MaxQuantity);
                        MapleItem item = MapleItemCreator.CreateItem(mobDrop.ItemID, source, (short)amount, true);
                        AddStolenItem(characterId, item.ItemId);
                        return item;
                    }
                }
            }
            return null;
        }
        #endregion
        #endregion

        #region Control
        public void SetController(MapleCharacter chr, int obbjectId, bool aggro)
        {
            MapleCharacter old;
            if (Controller.TryGetTarget(out old) && old != null && old != chr && chr.Map == old.Map)
            {
                old.Client.SendPacket(RemoveMobControl(obbjectId));
            }
            Controller.SetTarget(chr);
            ControllerHasAggro = aggro;
            chr.Client.SendPacket(SetMobControl(obbjectId, this));
        }

        public MapleCharacter GetController()
        {
            MapleCharacter chr;
            Controller.TryGetTarget(out chr);
            return chr;
        }

        public void ClearController()
        {
            Controller.SetTarget(null);
            ControllerHasAggro = false;
        }

        #endregion

        #region Packets
        public static PacketWriter SpawnMob(int objectId, MapleMonster mob, bool newSpawn = false) //updated v158
        {
            
            var pw = new PacketWriter(SMSGHeader.SPAWN_MONSTER);
            pw.WriteByte(0);//RED, no idea
            pw.WriteInt(objectId);
            pw.WriteBool(true); //No idea
            pw.WriteInt(mob.WzInfo.MobId);
            pw.WriteBool(true); //No idea

            pw.WriteInt(mob.WzInfo.HP);
            pw.WriteInt(mob.WzInfo.MP);
            pw.WriteInt(mob.WzInfo.Exp);
            pw.WriteInt(mob.WzInfo.PAD);
            pw.WriteInt(mob.WzInfo.MAD);
            pw.WriteInt(mob.WzInfo.PDRate);
            pw.WriteInt(mob.WzInfo.MDRate);
            pw.WriteInt(mob.WzInfo.Acc);
            pw.WriteInt(mob.WzInfo.Eva);
            pw.WriteInt(mob.WzInfo.Kb);
            pw.WriteInt(0); //?
            pw.WriteInt(mob.WzInfo.Level);
            pw.WriteInt(0); //?

            pw.WriteHexString("BF 02 00 60 00 00 00 FC 00 00 00 00 00 00 00 00");

            AddMobStatus(pw, mob);

            //[7C 01] [61 FE] [02] [84 00] [87 00] 
            pw.WriteShort((short)mob.Position.X);
            pw.WriteShort((short)(mob.Position.Y - 1));
            //pw.WritePoint(Mob.Position);
            pw.WriteByte(mob.Stance);

            pw.WriteShort(0); //current fh aka leave 0 else it looks they go back to respawn
            pw.WriteShort(mob.Fh); //initial fh

            pw.WriteByte(newSpawn ? (byte)0xFE : (byte)0xFF); //-1 = instant, -2 = fade in
            pw.WriteByte(0xFF); //carnival team
            pw.WriteByte(0x7D);
            pw.WriteZeroBytes(24);
            pw.WriteInt(-1);
            pw.WriteByte(0);
            pw.WriteZeroBytes(8);
            pw.WriteInt(-1);
            pw.WriteInt(0);
            return pw;
        }

        public static PacketWriter KillMob(int objectId)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.KILL_MONSTER);
            pw.WriteInt(objectId);
            pw.WriteByte(1); //animation, 1 = death animation, 0 = vanish I think
            return pw;
        }

        public static PacketWriter SetMobControl(int objectId, MapleMonster mob, bool newSpawn = false) //updated v142
        {
            
            var pw = new PacketWriter(SMSGHeader.SPAWN_MONSTER_CONTROL);
            pw.WriteByte((byte)(mob.ControllerHasAggro ? 2 : 1));
            pw.WriteInt(objectId);
            pw.WriteBool(true); //No idea
            pw.WriteInt(mob.WzInfo.MobId);
            pw.WriteBool(true); //No idea

            pw.WriteInt(mob.WzInfo.HP);
            pw.WriteInt(mob.WzInfo.MP);
            pw.WriteInt(mob.WzInfo.Exp);
            pw.WriteInt(mob.WzInfo.PAD);
            pw.WriteInt(mob.WzInfo.MAD);
            pw.WriteInt(mob.WzInfo.PDRate);
            pw.WriteInt(mob.WzInfo.MDRate);
            pw.WriteInt(mob.WzInfo.Acc);
            pw.WriteInt(mob.WzInfo.Eva);
            pw.WriteInt(mob.WzInfo.Kb);
            pw.WriteInt(0); //?
            pw.WriteInt(mob.WzInfo.Level);
            pw.WriteInt(0); //?
                            //pw.WriteZeroBytes(3);
            pw.WriteHexString("BF 02 00 60 00 00 00 FC 00 00 00 00 00 00 00 00");

            AddMobStatus(pw, mob);

            pw.WritePoint(mob.Position);
            //DF 01 C7 01 [02] [A1 00] [9C 00] [FF] [FF] [7D] [00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00] [FF FF FF FF] [00 00 00 00 00 00 00 00 00 00 00 00 00] [FF]
            pw.WriteByte(mob.Stance);
            pw.WriteShort(0);
            pw.WriteShort(mob.Fh);
            pw.WriteByte(newSpawn ? (byte)0xFE : (byte)0xFF);
            pw.WriteByte(0xFF); //carnival team
            pw.WriteByte(0x7D);
            pw.WriteZeroBytes(24);
            pw.WriteInt(-1);
            pw.WriteByte(3);
            pw.WriteZeroBytes(12);
            pw.WriteByte(0xFF);
            return pw;
        }

        public static PacketWriter RemoveMobControl(int objectid)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.SPAWN_MONSTER_CONTROL);
            pw.WriteByte(0);
            pw.WriteInt(objectid);
            return pw;
        }

        private static void AddMobStatus(PacketWriter pw, MapleMonster mob) //updated 158
        {
            pw.WriteHexString("58 03 00 00 00 00 00 00 00 00 58 03 00 00 00 00 00 00 00 00 58 03 00 00 00 00 00 00 00 00 58 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
        }

        public static PacketWriter UpdateHp(int objectId, byte hpPercentage)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.SHOW_MONSTER_HP);
            pw.WriteInt(objectId);
            pw.WriteByte(hpPercentage);
            return pw;
        }
        #endregion

        public static class MobStatusEffects
        {
            public static readonly MobBuffStat WAtk = new MobBuffStat(0);
            public static readonly MobBuffStat WDef = new MobBuffStat(1);
            public static readonly MobBuffStat MAtk = new MobBuffStat(2);
            public static readonly MobBuffStat MDef = new MobBuffStat(3);
            public static readonly MobBuffStat Acc = new MobBuffStat(4);
            public static readonly MobBuffStat Avoid = new MobBuffStat(5);
            public static readonly MobBuffStat Speed = new MobBuffStat(6);
            public static readonly MobBuffStat Stun = new MobBuffStat(7);
            public static readonly MobBuffStat Freeze = new MobBuffStat(8);
            public static readonly MobBuffStat Poison = new MobBuffStat(9);
            public static readonly MobBuffStat Seal = new MobBuffStat(10);
            public static readonly MobBuffStat Showdown = new MobBuffStat(11);
            public static readonly MobBuffStat WeaponAtkUp = new MobBuffStat(12);
            public static readonly MobBuffStat WeaponDefUp = new MobBuffStat(13);
            public static readonly MobBuffStat MagicAtkUp = new MobBuffStat(14);
            public static readonly MobBuffStat MagicDefUp = new MobBuffStat(15);
            public static readonly MobBuffStat Doom = new MobBuffStat(16);
            public static readonly MobBuffStat ShadowWeb = new MobBuffStat(17);
            public static readonly MobBuffStat WeaponImmune = new MobBuffStat(18);
            public static readonly MobBuffStat MagicImmune = new MobBuffStat(19);
            public static readonly MobBuffStat DamageImmune = new MobBuffStat(21);
            public static readonly MobBuffStat NinjaAmbush = new MobBuffStat(22);
            public static readonly MobBuffStat Burn = new MobBuffStat(24);
            public static readonly MobBuffStat Darkness = new MobBuffStat(25);
            public static readonly MobBuffStat Empty = new MobBuffStat(27);
            public static readonly MobBuffStat Hypnotize = new MobBuffStat(28);
            public static readonly MobBuffStat WAtkReflect = new MobBuffStat(29);
            public static readonly MobBuffStat MAtkReflect = new MobBuffStat(30);
            public static readonly MobBuffStat SummonBag = new MobBuffStat(31); //all summon bag mobs have this

            public static readonly MobBuffStat Neutralise = new MobBuffStat(33); // first int on v.87 or else it won't work.
            public static readonly MobBuffStat Imprint = new MobBuffStat(34);
            public static readonly MobBuffStat MonsterBomb = new MobBuffStat(35);
            public static readonly MobBuffStat MagicCrash = new MobBuffStat(36);
            public static readonly MobBuffStat EMPTY_1 = new MobBuffStat(37); //chaos
            public static readonly MobBuffStat EMPTY_2 = new MobBuffStat(38);
            public static readonly MobBuffStat EMPTY_3 = new MobBuffStat(39);
            public static readonly MobBuffStat EMPTY_4 = new MobBuffStat(40); //jump
            public static readonly MobBuffStat EMPTY_5 = new MobBuffStat(41);
            public static readonly MobBuffStat EMPTY_6 = new MobBuffStat(42);
            public static readonly MobBuffStat EMPTY_7 = new MobBuffStat(45);
        }
    }
}