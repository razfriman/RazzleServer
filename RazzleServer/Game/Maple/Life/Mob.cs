using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Mob : MapObject, IMoveable, ISpawnable, IControllable
    {
        public int MapleId { get; private set; }
        [JsonIgnore]
        public Character Controller { get; set; }
        public Dictionary<Character, uint> Attackers { get; private set; }
        public SpawnPoint SpawnPoint { get; private set; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public List<Loot> Loots { get; private set; }
        public short Foothold { get; set; }
        public MobSkills Skills { get; private set; }
        public Dictionary<MobSkill, DateTime> Cooldowns { get; private set; }
        public List<MobStatus> Buffs { get; private set; }
        public List<int> DeathSummons { get; private set; }

        public short Level { get; private set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; private set; }
        public uint MaxMana { get; private set; }
        public uint HealthRecovery { get; private set; }
        public uint ManaRecovery { get; private set; }
        public int ExplodeHealth { get; private set; }
        public uint Experience { get; private set; }
        public int Link { get; private set; }
        public short SummonType { get; private set; }
        public int FixedDamage { get; private set; }
        public int DeathBuff { get; private set; }
        public int DeathAfter { get; private set; }
        public double Traction { get; private set; }
        public bool DamagedByMobOnly { get; private set; }
        public int DropItemPeriod { get; private set; }
        public byte HpBarForeColor { get; private set; }
        public byte HpBarBackColor { get; private set; }
        public byte CarnivalPoints { get; private set; }
        public int WeaponAttack { get; private set; }
        public int WeaponDefense { get; private set; }
        public int MagicAttack { get; private set; }
        public int MagicDefense { get; private set; }
        public short Accuracy { get; private set; }
        public short Avoidability { get; private set; }
        public short Speed { get; private set; }
        public short ChaseSpeed { get; private set; }

        public bool IsFacingLeft => Stance % 2 == 0;

        public bool CanRespawn => true;

        public int SpawnEffect { get; set; }

        [JsonIgnore]
        public Mob CachedReference => DataProvider.Mobs.Data[MapleId];

        public Mob()
        {

        }

        public Mob(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;

            Level = img["level"]?.GetShort() ?? 1;
            MaxHealth = (uint)(img["maxHP"]?.GetInt() ?? 0);
            Health = MaxHealth;
            MaxMana = (uint)(img["maxMP"]?.GetInt() ?? 0);
            Mana = MaxHealth;
            Speed = img["speed"]?.GetShort() ?? 0;
            HpBarForeColor = (byte)(img["hpTagColor"]?.GetInt() ?? 0);
            HpBarBackColor = (byte)(img["hpTagBgcolor"]?.GetInt() ?? 0);
            SummonType = img["summonType"]?.GetShort() ?? 0;
            Link = img["link"]?.GetInt() ?? 0;
            WeaponAttack = img["PADamage"]?.GetInt() ?? 0;
            WeaponDefense = img["PDDamage"]?.GetInt() ?? 0;
            MagicAttack = img["MADamage"]?.GetInt() ?? 0;
            MagicDefense = img["MDDamage"]?.GetInt() ?? 0;
            Accuracy = img["acc"]?.GetShort() ?? 0;
            Avoidability = img["eva"]?.GetShort() ?? 0;
            Experience = (uint)(img["exp"]?.GetInt() ?? 0);
            HealthRecovery = (uint)(img["hpRecovery"]?.GetInt() ?? 0);
            ManaRecovery = (uint)(img["mpRecovery"]?.GetInt() ?? 0);
            ChaseSpeed = img["chaseSpeed"]?.GetShort() ?? 0;
            FixedDamage = img["fixedDamage"]?.GetInt() ?? 0;
            DropItemPeriod = img["dropItemPeriod"]?.GetInt() ?? 0;
            DamagedByMobOnly = (img["damagedByMob"]?.GetInt() ?? 0) > 0;
            Traction = img["fs"]?.GetDouble() ?? 0d;
            DeathAfter = img["removeAfter"]?.GetInt() ?? 0;

            //publicReward
            //explosiveReward
            //HPgaugeHide
            //firstAttack
            //boss
            //undead
            //pushed
            //bodyAttack
            //elemAttr
            //noFlip
            //damagedBySelectedMob/0 = 9300150
            //doNotRemove
            //onlyNormalAttack
            //buff

            //ExplodeHealth = (int)datum["explode_hp"];
            //DeathBuff = (int)datum["death_buff"];

            Loots = new List<Loot>();
            Skills = new MobSkills(this);
            DeathSummons = new List<int>();

            img["skill"]?.WzProperties.ForEach(x => Skills.Add(new MobSkill(x)));
            img["revive"]?.WzProperties?.ForEach(x => DeathSummons.Add(x.GetInt()));
        }

        public Mob(int mapleId)
        {
            MapleId = mapleId;

            Level = CachedReference.Level;
            Health = CachedReference.Health;
            Mana = CachedReference.Mana;
            MaxHealth = CachedReference.MaxHealth;
            MaxMana = CachedReference.MaxMana;
            HealthRecovery = CachedReference.HealthRecovery;
            ManaRecovery = CachedReference.ManaRecovery;
            ExplodeHealth = CachedReference.ExplodeHealth;
            Experience = CachedReference.Experience;
            Link = CachedReference.Link;
            SummonType = CachedReference.SummonType;
            FixedDamage = CachedReference.FixedDamage;
            DeathBuff = CachedReference.DeathBuff;
            DeathAfter = CachedReference.DeathAfter;
            Traction = CachedReference.Traction;
            DamagedByMobOnly = CachedReference.DamagedByMobOnly;
            DropItemPeriod = CachedReference.DropItemPeriod;
            HpBarForeColor = CachedReference.HpBarForeColor;
            HpBarBackColor = CachedReference.HpBarBackColor;
            CarnivalPoints = CachedReference.CarnivalPoints;
            WeaponAttack = CachedReference.WeaponAttack;
            WeaponDefense = CachedReference.WeaponDefense;
            MagicAttack = CachedReference.MagicAttack;
            MagicDefense = CachedReference.MagicDefense;
            Accuracy = CachedReference.Accuracy;
            Avoidability = CachedReference.Avoidability;
            Speed = CachedReference.Speed;
            ChaseSpeed = CachedReference.ChaseSpeed;

            Loots = CachedReference.Loots;
            Skills = CachedReference.Skills;
            DeathSummons = CachedReference.DeathSummons;

            Attackers = new Dictionary<Character, uint>();
            Cooldowns = new Dictionary<MobSkill, DateTime>();
            Buffs = new List<MobStatus>();
            Stance = 5;
            CanDrop = true;
        }

        public Mob(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleId)
        {
            SpawnPoint = spawnPoint;
            Foothold = SpawnPoint.Foothold;
            Position = SpawnPoint.Position;
            Position.Y -= 1; // TODO: Is this needed?
        }

        public Mob(int mapleId, Point position)
            : this(mapleId)
        {
            Foothold = 0; // TODO.
            Position = position;
            Position.Y -= 5; // TODO: Is this needed?
        }

        public void AssignController()
        {
            if (Controller == null)
            {
                var leastControlled = int.MaxValue;
                Character newController = null;

                lock (Map.Characters)
                {
                    foreach (var character in Map.Characters.Values)
                    {
                        if (character.ControlledMobs.Count < leastControlled)
                        {
                            leastControlled = character.ControlledMobs.Count;
                            newController = character;
                        }
                    }
                }

                if (newController != null)
                {
                    IsProvoked = false;

                    newController.ControlledMobs.Add(this);
                }
            }
        }

        public void SwitchController(Character newController)
        {
            lock (this)
            {
                if (Controller != newController)
                {
                    Controller.ControlledMobs.Remove(this);

                    newController.ControlledMobs.Add(this);
                }
            }
        }

        public void Move(PacketReader iPacket)
        {
            var moveAction = iPacket.ReadShort();
            var skillByte = iPacket.ReadByte();
            var skillId = iPacket.ReadInt();
            iPacket.ReadShort();
            iPacket.ReadInt();

            var movements = new Movements(iPacket);

            Position = movements.Position;
            Foothold = movements.Foothold;
            Stance = movements.Stance;

            // TODO - Load the mob skill
            MobSkill skill = null;
            if (skill != null)
            {
                if (Health * 100 / MaxHealth > skill.PercentageLimitHP ||
                    Cooldowns.ContainsKey(skill) && Cooldowns[skill].AddSeconds(skill.Cooldown) >= DateTime.Now ||
                    (MobSkillName)skill.MapleId == MobSkillName.Summon && Map.Mobs.Count >= 100)
                {
                    skill = null;
                }
            }

            if (skill != null)
            {
                skill.Cast(this);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MobMoveResponse))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteShort(moveAction);
                oPacket.WriteBool(skill != null); // use skills
                oPacket.WriteShort((short)Mana);
                oPacket.WriteShort(0); // skill id, skill level

                Controller.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MobMove))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteBool(skill != null); // use skills
                oPacket.WriteInt(skillId);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(movements.ToByteArray());

                Map.Broadcast(oPacket, Controller);
            }
        }

        public void Buff(MobStatus buff, short value, MobSkill skill)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.MobStatSet))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteLong(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt((int)buff);
                oPacket.WriteShort(value);
                oPacket.WriteShort(skill.MapleId);
                oPacket.WriteShort(skill.Level);
                oPacket.WriteShort(-1);
                oPacket.WriteShort(0);// Delay
                oPacket.WriteInt(0);

                Map.Broadcast(oPacket);
            }

            Delay.Execute(() =>
            {
                using (var packet = new PacketWriter(ServerOperationCode.MobStatReset))
                {
                    packet.WriteInt(ObjectId);
                    packet.WriteLong(0);
                    packet.WriteInt(0);
                    packet.WriteInt((int)buff);
                    packet.WriteInt(0);

                    Map.Broadcast(packet);
                }

                Buffs.Remove(buff);
            }, skill.Duration * 1000);
        }

        public void Heal(uint hp, int range)
        {
            Health = Math.Min(MaxHealth, (uint)(Health + hp + Functions.Random(-range / 2, range / 2)));

            using (var packet = new PacketWriter(ServerOperationCode.MobDamaged))
            {
                packet.WriteInt(ObjectId);
                packet.WriteByte(0);
                packet.WriteInt((int)-hp);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
                Map.Broadcast(packet);
            }
        }

        public void Die()
        {
            if (!Map.Mobs.Remove(this))
            {

            }
        }

        public bool Damage(Character attacker, uint amount)
        {
            lock (this)
            {
                var originalAmount = amount;

                amount = Math.Min(amount, Health);

                if (Attackers.ContainsKey(attacker))
                {
                    Attackers[attacker] += amount;
                }
                else
                {
                    Attackers.Add(attacker, amount);
                }

                Health -= amount;

                using (var oPacket = new PacketWriter(ServerOperationCode.MobHPIndicator))
                {
                    oPacket.WriteInt(ObjectId);
                    oPacket.WriteByte((byte)(Health * 100 / MaxHealth));

                    attacker.Client.Send(oPacket);
                }

                if (Health <= 0)
                {
                    return true;
                }

                return false;
            }
        }

        public PacketWriter GetCreatePacket() => GetInternalPacket(false, true);

        public PacketWriter GetSpawnPacket() => GetInternalPacket(false, false);

        public PacketWriter GetControlRequestPacket() => GetInternalPacket(true, false);

        private PacketWriter GetInternalPacket(bool requestControl, bool newSpawn)
        {
            var oPacket = new PacketWriter(requestControl ? ServerOperationCode.MobChangeController : ServerOperationCode.MobEnterField);

            if (requestControl)
            {
                oPacket.WriteByte((byte)(IsProvoked ? 2 : 1));
            }

            oPacket.WriteInt(ObjectId);
            oPacket.WriteByte((byte)(Controller == null ? 5 : 1));
            oPacket.WriteInt(MapleId);
            oPacket.WriteInt(0);
            oPacket.WritePoint(Position);
            oPacket.WriteByte((byte)(0x02 | (IsFacingLeft ? 0x01 : 0x00)));
            oPacket.WriteShort(0);
            oPacket.WriteShort(Foothold);

            if (SpawnEffect > 0)
            {
                oPacket.WriteByte((byte)SpawnEffect);
                oPacket.WriteByte(0);
                oPacket.WriteShort(0);
            }

            oPacket.WriteByte((byte)(newSpawn ? -2 : -1));
            oPacket.WriteByte(0);

            return oPacket;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobChangeController);

            oPacket.WriteBool(false);
            oPacket.WriteInt(ObjectId);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobLeaveField);
            var animated = true;
            oPacket.WriteInt(ObjectId);
            oPacket.WriteBool(animated);

            return oPacket;
        }
    }
}
