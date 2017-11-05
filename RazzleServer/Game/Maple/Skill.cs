using System;
using System.Linq;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.WzLib;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Skill
    {
        public CharacterSkills Parent { get; set; }

        private byte currentLevel;
        private byte maxLevel;
        private DateTime cooldownEnd = DateTime.MinValue;

        public int ID { get; set; }
        public int MapleID { get; set; }
        public DateTime Expiration { get; set; }

        public sbyte MobCount { get; set; }
        public sbyte HitCount { get; set; }
        public short Range { get; set; }
        public int BuffTime { get; set; }
        public short CostMP { get; set; }
        public short CostHP { get; set; }
        public short Damage { get; set; }
        public int FixedDamage { get; set; }
        public byte CriticalDamage { get; set; }
        public sbyte Mastery { get; set; }
        public int OptionalItemCost { get; set; }
        public int CostItem { get; set; }
        public short ItemCount { get; set; }
        public short CostBullet { get; set; }
        public short CostMeso { get; set; }
        public short ParameterA { get; set; }
        public short ParameterB { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Strength { get; set; }
        public short WeaponAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicAttack { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }
        public short Probability { get; set; }
        public short Morph { get; set; }
        public Point LT { get; private set; }
        public Point RB { get; private set; }
        public int Cooldown { get; set; }

        public bool HasBuff => BuffTime > 0;

        public byte CurrentLevel
        {
            get => currentLevel;
            set
            {
                currentLevel = value;

                if (Parent != null)
                {
                    Recalculate();

                    if (Character.IsInitialized)
                    {
                        Update();
                    }
                }
            }
        }

        public byte MaxLevel
        {
            get => maxLevel;
            set
            {
                maxLevel = value;

                if (Parent != null && Character.IsInitialized)
                {
                    Update();
                }
            }
        }

        public Skill CachedReference => DataProvider.Skills[MapleID][CurrentLevel];

        public Character Character => Parent.Parent;

        public bool IsFromFourthJob => MapleID > 1000000 && (MapleID / 10000).ToString()[2] == '2'; // TODO: Redo that.

        public bool IsFromBeginner
        {
            get
            {
                return MapleID % 10000000 > 999 && MapleID % 10000000 < 1003;
            }
        }

        public bool IsCoolingDown => DateTime.Now < CooldownEnd;

        public int RemainingCooldownSeconds => Math.Min(0, (int)(CooldownEnd - DateTime.Now).TotalSeconds);

        public DateTime CooldownEnd
        {
            get => cooldownEnd;
            set
            {
                cooldownEnd = value;

                if (IsCoolingDown)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.Cooldown))
                    {
                        oPacket.WriteInt(MapleID);
                        oPacket.WriteShort((short)RemainingCooldownSeconds);

                        Character.Client.Send(oPacket);
                    }

                    Delay.Execute(() =>
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.Cooldown))
                        {
                            oPacket.WriteInt(MapleID);
                            oPacket.WriteShort(0);

                            Character.Client.Send(oPacket);
                        }
                    }, (RemainingCooldownSeconds * 1000));
                }
            }
        }

        private bool Assigned { get; set; }

        public Skill(int mapleID, DateTime? expiration = null)
        {
            MapleID = mapleID;
            CurrentLevel = 0;
            MaxLevel = (byte)DataProvider.Skills[MapleID].Count;

            if (!expiration.HasValue)
            {
                expiration = new DateTime(2079, 1, 1, 12, 0, 0); // NOTE: Default expiration time (permanent).
            }

            Expiration = (DateTime)expiration;
        }

        public Skill(int mapleID, byte currentLevel, byte maxLevel, DateTime? expiration = null)
        {
            MapleID = mapleID;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;

            if (!expiration.HasValue)
            {
                expiration = new DateTime(2079, 1, 1, 12, 0, 0); // NOTE: Default expiration time (permanent).
            }

            Expiration = (DateTime)expiration;
        }

        public Skill(WzImageProperty img)
        {
            //MapleID = (int)datum["skillid"];
            //CurrentLevel = (byte)(short)datum["skill_level"];
            //MobCount = (sbyte)datum["mob_count"];
            //HitCount = (sbyte)datum["hit_count"];
            //Range = (short)datum["range"];
            //BuffTime = (int)datum["buff_time"];
            //CostHP = (short)datum["hp_cost"];
            //CostMP = (short)datum["mp_cost"];
            //Damage = (short)datum["damage"];
            //FixedDamage = (int)datum["fixed_damage"];
            //CriticalDamage = (byte)datum["critical_damage"];
            //Mastery = (sbyte)datum["mastery"];
            //OptionalItemCost = (int)datum["optional_item_cost"];
            //CostItem = (int)datum["item_cost"];
            //ItemCount = (short)datum["item_count"];
            //CostBullet = (short)datum["bullet_cost"];
            //CostMeso = (short)datum["money_cost"];
            //ParameterA = (short)datum["x_property"];
            //ParameterB = (short)datum["y_property"];
            //Speed = (short)datum["speed"];
            //Jump = (short)datum["jump"];
            //Strength = (short)datum["str"];
            //WeaponAttack = (short)datum["weapon_atk"];
            //MagicAttack = (short)datum["magic_atk"];
            //WeaponDefense = (short)datum["weapon_def"];
            //MagicDefense = (short)datum["magic_def"];
            //Accuracy = (short)datum["accuracy"];
            //Avoidability = (short)datum["avoid"];
            //HP = (short)datum["hp"];
            //MP = (short)datum["mp"];
            //Probability = (short)datum["prop"];
            //Morph = (short)datum["morph"];
            //LT = new Point((short)datum["ltx"], (short)datum["lty"]);
            //RB = new Point((short)datum["rbx"], (short)datum["rby"]);
            //Cooldown = (int)datum["cooldown_time"];
        }
        public Skill(Datum datum)
        {
            ID = (int)datum["ID"];
            Assigned = true;

            MapleID = (int)datum["MapleID"];
            CurrentLevel = (byte)datum["CurrentLevel"];
            MaxLevel = (byte)datum["MaxLevel"];
            Expiration = (DateTime)datum["Expiration"];
            CooldownEnd = (DateTime)datum["CooldownEnd"];
        }

        public void Save()
        {
            Datum datum = new Datum("skills");

            datum["CharacterID"] = Character.ID;
            datum["MapleID"] = MapleID;
            datum["CurrentLevel"] = CurrentLevel;
            datum["MaxLevel"] = MaxLevel;
            datum["Expiration"] = Expiration;
            datum["CooldownEnd"] = CooldownEnd;

            if (Assigned)
            {
                datum.Update("ID = {0}", ID);
            }
            else
            {
                ID = datum.InsertAndReturnID();
                Assigned = true;
            }
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var skill = dbContext.Skills.FirstOrDefault(x => x.ID == ID);
                if (skill != null)
                {
                    dbContext.Skills.Remove(skill);
                }
            }

            Assigned = false;
        }

        public void Update()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.ChangeSkillRecordResult))
            {
                oPacket.WriteByte(1);
                oPacket.WriteShort(1);
                oPacket.WriteInt(MapleID);
                oPacket.WriteInt(CurrentLevel);
                oPacket.WriteInt(MaxLevel);
                oPacket.WriteDateTime(Expiration);
                oPacket.WriteByte(4);

                Character.Client.Send(oPacket);
            }
        }

        public void Recalculate()
        {
            MobCount = CachedReference.MobCount;
            HitCount = CachedReference.HitCount;
            Range = CachedReference.Range;
            BuffTime = CachedReference.BuffTime;
            CostMP = CachedReference.CostMP;
            CostHP = CachedReference.CostHP;
            Damage = CachedReference.Damage;
            FixedDamage = CachedReference.FixedDamage;
            CriticalDamage = CachedReference.CriticalDamage;
            Mastery = CachedReference.Mastery;
            OptionalItemCost = CachedReference.OptionalItemCost;
            CostItem = CachedReference.CostItem;
            ItemCount = CachedReference.ItemCount;
            CostBullet = CachedReference.CostBullet;
            CostMeso = CachedReference.CostMeso;
            ParameterA = CachedReference.ParameterA;
            ParameterB = CachedReference.ParameterB;
            Speed = CachedReference.Speed;
            Jump = CachedReference.Jump;
            Strength = CachedReference.Strength;
            WeaponAttack = CachedReference.WeaponAttack;
            WeaponDefense = CachedReference.WeaponDefense;
            MagicAttack = CachedReference.MagicAttack;
            MagicDefense = CachedReference.MagicDefense;
            Accuracy = CachedReference.Accuracy;
            Avoidability = CachedReference.Avoidability;
            HP = CachedReference.HP;
            MP = CachedReference.MP;
            Probability = CachedReference.Probability;
            Morph = CachedReference.Morph;
            LT = CachedReference.LT;
            RB = CachedReference.RB;
            Cooldown = CachedReference.Cooldown;
        }

        public void Cast()
        {
            if (IsCoolingDown)
            {
                return;
            }

            Character.Health -= CostHP;
            Character.Mana -= CostMP;

            if (CostItem > 0)
            {

            }

            if (CostBullet > 0)
            {

            }

            if (CostMeso > 0)
            {

            }

            if (Cooldown > 0)
            {
                CooldownEnd = DateTime.Now.AddSeconds(Cooldown);
            }
        }

        //public void Cast(PacketReader iPacket)
        //{
        //    if (!this.Character.IsAlive)
        //    {
        //        return;
        //    }

        //    if (this.IsCoolingDown)
        //    {
        //        return;
        //    }

        //    if (this.MapleID == (int)SkillNames.Priest.MysticDoor)
        //    {
        //        Point origin = new Point(iPacket.ReadShort(), iPacket.ReadShort());

        //        // TODO: Open mystic door.
        //    }

        //    this.Character.Health -= this.CostHP;
        //    this.Character.Mana -= this.CostMP;

        //    if (this.Cooldown > 0)
        //    {
        //        this.CooldownEnd = DateTime.Now.AddSeconds(this.Cooldown);
        //    }

        //    // TODO: Money cost.

        //    byte type = 0;
        //    byte direction = 0;
        //    short addedInfo = 0;

        //    switch (this.MapleID)
        //    {
        //        case (int)SkillNames.Priest.MysticDoor:
        //            // NOTe: Prevents the default case from executing, there's no packet data left for it.
        //            break;

        //        case (int)SkillNames.Brawler.MpRecovery:
        //            {
        //                short healthMod = (short)((this.Character.MaxHealth * this.ParameterA) / 100);
        //                short manaMod = (short)((healthMod * this.ParameterB) / 100);

        //                this.Character.Health -= healthMod;
        //                this.Character.Mana += manaMod;
        //            }
        //            break;

        //        case (int)SkillNames.Shadower.Smokescreen:
        //            {
        //                Point origin = new Point(iPacket.ReadShort(), iPacket.ReadShort());

        //                // TODO: Mists.
        //            }
        //            break;

        //        case (int)SkillNames.Corsair.Battleship:
        //            {
        //                // TODO: Reset Battleship health.
        //            }
        //            break;

        //        case (int)SkillNames.Crusader.ArmorCrash:
        //        case (int)SkillNames.WhiteKnight.MagicCrash:
        //        case (int)SkillNames.DragonKnight.PowerCrash:
        //            {
        //                iPacket.ReadInt(); // NOTE: Unknown, probably CRC.
        //                byte mobs = iPacket.ReadByte();

        //                for (byte i = 0; i < mobs; i++)
        //                {
        //                    int objectID = iPacket.ReadInt();

        //                    Mob mob;

        //                    try
        //                    {
        //                        mob = this.Character.Map.Mobs[objectID];
        //                    }
        //                    catch (KeyNotFoundException)
        //                    {
        //                        return;
        //                    }

        //                    // TODO: Mob crash skill.
        //                }
        //            }
        //            break;

        //        case (int)SkillNames.Hero.MonsterMagnet:
        //        case (int)SkillNames.Paladin.MonsterMagnet:
        //        case (int)SkillNames.DarkKnight.MonsterMagnet:
        //            {
        //                int mobs = iPacket.ReadInt();

        //                for (int i = 0; i < mobs; i++)
        //                {
        //                    int objectID = iPacket.ReadInt();

        //                    Mob mob;

        //                    try
        //                    {
        //                        mob = this.Character.Map.Mobs[objectID];
        //                    }
        //                    catch (KeyNotFoundException)
        //                    {
        //                        return;
        //                    }

        //                    bool success = iPacket.ReadBool();

        //                    // TODO: Packet.
        //                }

        //                direction = iPacket.ReadByte();
        //            }
        //            break;

        //        case (int)SkillNames.FirePoisonWizard.Slow:
        //        case (int)SkillNames.IceLightningWizard.Slow:
        //        case (int)SkillNames.BlazeWizard.Slow:
        //        case (int)SkillNames.Page.Threaten:
        //            {
        //                iPacket.ReadInt(); // NOTE: Unknown, probably CRC.

        //                byte mobs = iPacket.ReadByte();

        //                for (byte i = 0; i < mobs; i++)
        //                {
        //                    int objectID = iPacket.ReadInt();

        //                    Mob mob;

        //                    try
        //                    {
        //                        mob = this.Character.Map.Mobs[objectID];
        //                    }
        //                    catch (KeyNotFoundException)
        //                    {
        //                        return;
        //                    }
        //                }

        //                // TODO: Apply mob status.
        //            }
        //            break;

        //        case (int)SkillNames.FirePoisonMage.Seal:
        //        case (int)SkillNames.IceLightningMage.Seal:
        //        case (int)SkillNames.BlazeWizard.Seal:
        //        case (int)SkillNames.Priest.Doom:
        //        case (int)SkillNames.Hermit.ShadowWeb:
        //        case (int)SkillNames.NightWalker.ShadowWeb:
        //        case (int)SkillNames.Shadower.NinjaAmbush:
        //        case (int)SkillNames.NightLord.NinjaAmbush:
        //            {
        //                byte mobs = iPacket.ReadByte();

        //                for (byte i = 0; i < mobs; i++)
        //                {
        //                    int objectID = iPacket.ReadInt();

        //                    Mob mob;

        //                    try
        //                    {
        //                        mob = this.Character.Map.Mobs[objectID];
        //                    }
        //                    catch (KeyNotFoundException)
        //                    {
        //                        return;
        //                    }
        //                }

        //                // TODO: Apply mob status.
        //            }
        //            break;

        //        case (int)SkillNames.Bishop.HerosWill:
        //        case (int)SkillNames.IceLightningArchMage.HerosWill:
        //        case (int)SkillNames.FirePoisonArchMage.HerosWill:
        //        case (int)SkillNames.DarkKnight.HerosWill:
        //        case (int)SkillNames.Hero.HerosWill:
        //        case (int)SkillNames.Paladin.HerosWill:
        //        case (int)SkillNames.NightLord.HerosWill:
        //        case (int)SkillNames.Shadower.HerosWill:
        //        case (int)SkillNames.Bowmaster.HerosWill:
        //        case (int)SkillNames.Marksman.HerosWill:
        //            {
        //                // TODO: Add Buccaneer & Corsair.

        //                // TODO: Remove Sedcude debuff.
        //            }
        //            break;

        //        case (int)SkillNames.Priest.Dispel:
        //            {

        //            }
        //            break;

        //        case (int)SkillNames.Cleric.Heal:
        //            {
        //                short healthRate = this.HP;

        //                if (healthRate > 100)
        //                {
        //                    healthRate = 100;
        //                }

        //                int partyPlayers = this.Character.Party != null ? this.Character.Party.Count : 1;
        //                short healthMod = (short)(((healthRate * this.Character.MaxHealth) / 100) / partyPlayers);

        //                if (this.Character.Party != null)
        //                {
        //                    int experience = 0;

        //                    List<PartyMember> members = new List<PartyMember>();

        //                    foreach (PartyMember member in this.Character.Party)
        //                    {
        //                        if (member.Character != null && member.Character.Map.MapleID == this.Character.Map.MapleID)
        //                        {
        //                            members.Add(member);
        //                        }
        //                    }

        //                    foreach (PartyMember member in members)
        //                    {
        //                        short memberHealth = member.Character.Health;

        //                        if (memberHealth > 0 && memberHealth < member.Character.MaxHealth)
        //                        {
        //                            member.Character.Health += healthMod;

        //                            if (member.Character != this.Character)
        //                            {
        //                                experience += 20 * (member.Character.Health - memberHealth) / (8 * member.Character.Level + 190);
        //                            }
        //                        }
        //                    }

        //                    if (experience > 0)
        //                    {
        //                        this.Character.Experience += experience;
        //                    }
        //                }
        //                else
        //                {
        //                    this.Character.Health += healthRate;
        //                }
        //            }
        //            break;

        //        case (int)SkillNames.Fighter.Rage:
        //        case (int)SkillNames.DawnWarrior.Rage:
        //        case (int)SkillNames.Spearman.IronWill:
        //        case (int)SkillNames.Spearman.HyperBody:
        //        case (int)SkillNames.FirePoisonWizard.Meditation:
        //        case (int)SkillNames.IceLightningWizard.Meditation:
        //        case (int)SkillNames.BlazeWizard.Meditation:
        //        case (int)SkillNames.Cleric.Bless:
        //        case (int)SkillNames.Priest.HolySymbol:
        //        case (int)SkillNames.Bishop.Resurrection:
        //        case (int)SkillNames.Bishop.HolyShield:
        //        case (int)SkillNames.Bowmaster.SharpEyes:
        //        case (int)SkillNames.Marksman.SharpEyes:
        //        case (int)SkillNames.Assassin.Haste:
        //        case (int)SkillNames.NightWalker.Haste:
        //        case (int)SkillNames.Hermit.MesoUp:
        //        case (int)SkillNames.Bandit.Haste:
        //        case (int)SkillNames.Buccaneer.SpeedInfusion:
        //        case (int)SkillNames.ThunderBreaker.SpeedInfusion:
        //        case (int)SkillNames.Buccaneer.TimeLeap:
        //        case (int)SkillNames.Hero.MapleWarrior:
        //        case (int)SkillNames.Paladin.MapleWarrior:
        //        case (int)SkillNames.DarkKnight.MapleWarrior:
        //        case (int)SkillNames.FirePoisonArchMage.MapleWarrior:
        //        case (int)SkillNames.IceLightningArchMage.MapleWarrior:
        //        case (int)SkillNames.Bishop.MapleWarrior:
        //        case (int)SkillNames.Bowmaster.MapleWarrior:
        //        case (int)SkillNames.Marksman.MapleWarrior:
        //        case (int)SkillNames.NightLord.MapleWarrior:
        //        case (int)SkillNames.Shadower.MapleWarrior:
        //        case (int)SkillNames.Buccaneer.MapleWarrior:
        //        case (int)SkillNames.Corsair.MapleWarrior:
        //            {
        //                if (this.MapleID == (int)SkillNames.Buccaneer.TimeLeap)
        //                {
        //                    // TODO: Remove all cooldowns.
        //                }

        //                if (this.Character.Party != null)
        //                {
        //                    byte targets = iPacket.ReadByte();

        //                    // TODO: Get affected party members.

        //                    List<PartyMember> affected = new List<PartyMember>();

        //                    foreach (PartyMember member in affected)
        //                    {
        //                        using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
        //                        {
        //                            oPacket
        //                                .WriteByte((byte)UserEffect.SkillAffected)
        //                                .WriteInt(this.MapleID)
        //                                .WriteByte(1)
        //                                .WriteByte(1);

        //                            member.Character.Client.Send(oPacket);
        //                        }

        //                        using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
        //                        {
        //                            oPacket
        //                                .WriteInt(member.Character.ID)
        //                                .WriteByte((byte)UserEffect.SkillAffected)
        //                                .WriteInt(this.MapleID)
        //                                .WriteByte(1)
        //                                .WriteByte(1);

        //                            member.Character.Map.Broadcast(oPacket, member.Character);
        //                        }

        //                        member.Character.Buffs.Add(this, 0);
        //                    }
        //                }
        //            }
        //            break;

        //        case (int)SkillNames.Beginner.EchoOfHero:
        //        case (int)SkillNames.Noblesse.EchoOfHero:
        //        case (int)SkillNames.SuperGM.Haste:
        //        case (int)SkillNames.SuperGM.HolySymbol:
        //        case (int)SkillNames.SuperGM.Bless:
        //        case (int)SkillNames.SuperGM.HyperBody:
        //        case (int)SkillNames.SuperGM.HealPlusDispel:
        //        case (int)SkillNames.SuperGM.Resurrection:
        //            {
        //                byte targets = iPacket.ReadByte();
        //                Func<Character, bool> condition = null;
        //                Action<Character> action = null;

        //                switch (this.MapleID)
        //                {
        //                    case (int)SkillNames.SuperGM.HealPlusDispel:
        //                        {
        //                            condition = new Func<Character, bool>((target) => target.IsAlive);
        //                            action = new Action<Character>((target) =>
        //                            {
        //                                target.Health = target.MaxHealth;
        //                                target.Mana = target.MaxMana;

        //                                // TODO: Use dispell.
        //                            });
        //                        }
        //                        break;

        //                    case (int)SkillNames.SuperGM.Resurrection:
        //                        {
        //                            condition = new Func<Character, bool>((target) => !target.IsAlive);
        //                            action = new Action<Character>((target) =>
        //                            {
        //                                target.Health = target.MaxHealth;
        //                            });
        //                        }
        //                        break;

        //                    default:
        //                        {
        //                            condition = new Func<Character, bool>((target) => true);
        //                            action = new Action<Character>((target) =>
        //                            {
        //                                target.Buffs.Add(this, 0);
        //                            });
        //                        }
        //                        break;
        //                }

        //                for (byte i = 0; i < targets; i++)
        //                {
        //                    int targetID = iPacket.ReadInt();

        //                    Character target = this.Character.Map.Characters[targetID];

        //                    if (target != this.Character && condition(target))
        //                    {
        //                        using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
        //                        {
        //                            oPacket
        //                                .WriteByte((byte)UserEffect.SkillAffected)
        //                                .WriteInt(this.MapleID)
        //                                .WriteByte(1)
        //                                .WriteByte(1);

        //                            target.Client.Send(oPacket);
        //                        }

        //                        using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
        //                        {
        //                            oPacket
        //                                .WriteInt(target.ID)
        //                                .WriteByte((byte)UserEffect.SkillAffected)
        //                                .WriteInt(this.MapleID)
        //                                .WriteByte(1)
        //                                .WriteByte(1);

        //                            target.Map.Broadcast(oPacket, target);
        //                        }

        //                        action(target);
        //                    }
        //                }
        //            }
        //            break;

        //        default:
        //            {
        //                type = iPacket.ReadByte();

        //                switch (type)
        //                {
        //                    case 0x80:
        //                        addedInfo = iPacket.ReadShort();
        //                        break;
        //                }
        //            }
        //            break;
        //    }

        //    using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
        //    {
        //        oPacket
        //            .WriteByte((byte)UserEffect.SkillUse)
        //            .WriteInt(this.MapleID)
        //            .WriteByte(1)
        //            .WriteByte(1);

        //        this.Character.Client.Send(oPacket);
        //    }

        //    using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
        //    {
        //        oPacket
        //            .WriteInt(Character.ID)
        //            .WriteByte((byte)UserEffect.SkillUse)
        //            .WriteInt(this.MapleID)
        //            .WriteByte(1)
        //            .WriteByte(1);

        //        this.Character.Map.Broadcast(oPacket, this.Character);
        //    }

        //    if (this.HasBuff)
        //    {
        //        this.Character.Buffs.Add(this, 0);
        //    }
        //}

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(MapleID);
                oPacket.WriteInt(CurrentLevel);
                oPacket.WriteDateTime(Expiration);

                if (IsFromFourthJob)
                {
                    oPacket.WriteInt(MaxLevel);
                }

                return oPacket.ToArray();
            }
        }
    }
}
