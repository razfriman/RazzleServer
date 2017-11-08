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

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(MapleID);
                oPacket.WriteInt(CurrentLevel);

                if (IsFromFourthJob)
                {
                    oPacket.WriteInt(MaxLevel);
                }

                return oPacket.ToArray();
            }
        }
    }
}
