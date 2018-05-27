using System;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
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

        public int Id { get; set; }
        public int MapleId { get; set; }
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

        public SkillReference CachedReference => DataProvider.Skills.Data[MapleId][CurrentLevel];

        public Character Character => Parent.Parent;

        public bool IsFromFourthJob => MapleId > 1000000 && (MapleId / 10000).ToString()[2] == '2'; // TODO: Redo that.

        public bool IsFromBeginner => MapleId % 10000000 > 999 && MapleId % 10000000 < 1003;

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
                    Character.Client.Send(GamePackets.Cooldown(MapleId, RemainingCooldownSeconds));

                    Delay.Execute(() =>
                    {
                        Character.Client.Send(GamePackets.Cooldown(MapleId, 0));
                    }, RemainingCooldownSeconds * 1000);
                }
            }
        }

        private bool Assigned { get; set; }

        public Skill()
        {

        }

        public Skill(int mapleId, DateTime? expiration = null)
        {
            MapleId = mapleId;
            CurrentLevel = 0;
            MaxLevel = (byte)DataProvider.Skills.Data[MapleId].Count;

            if (!expiration.HasValue)
            {
                expiration = new DateTime(2079, 1, 1, 12, 0, 0); // NOTE: Default expiration time (permanent).
            }

            Expiration = (DateTime)expiration;
        }

        public Skill(int mapleId, byte currentLevel, byte maxLevel, DateTime? expiration = null)
        {
            MapleId = mapleId;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;

            if (!expiration.HasValue)
            {
                expiration = new DateTime(2079, 1, 1, 12, 0, 0); // NOTE: Default expiration time (permanent).
            }

            Expiration = (DateTime)expiration;
        }

       
        public Skill(SkillEntity entity)
        {
            //Id = (int)entity["Id"];
            //Assigned = true;
            //MapleId = (int)entity["MapleId"];
            //CurrentLevel = (byte)entity["CurrentLevel"];
            //MaxLevel = (byte)entity["MaxLevel"];
            //Expiration = (DateTime)entity["Expiration"];
            //CooldownEnd = (DateTime)entity["CooldownEnd"];
        }

        public void Save()
        {
            //Datum datum = new Datum("skills");

            //datum["CharacterId"] = Character.Id;
            //datum["MapleId"] = MapleId;
            //datum["CurrentLevel"] = CurrentLevel;
            //datum["MaxLevel"] = MaxLevel;
            //datum["Expiration"] = Expiration;
            //datum["CooldownEnd"] = CooldownEnd;

            //if (Assigned)
            //{
            //    datum.Update("Id = {0}", Id);
            //}
            //else
            //{
            //    Id = datum.InsertAndReturnId();
            //    Assigned = true;
            //}
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var skill = dbContext.Skills.FirstOrDefault(x => x.Id == Id);
                if (skill != null)
                {
                    dbContext.Skills.Remove(skill);
                }
            }

            Assigned = false;
        }

        public void Update() => Character.Client.Send(
            GamePackets.ChangeSkillRecordResult(MapleId, CurrentLevel, MaxLevel, Expiration)
            );

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
                oPacket.WriteInt(MapleId);
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
