using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Skills
{
    public sealed class Skill
    {
        public CharacterSkills Parent { get; set; }

        private byte _currentLevel;
        private byte _maxLevel;

        public int Id { get; set; }
        public int MapleId { get; set; }
        public DateTime Expiration { get; set; }

        public sbyte MobCount { get; set; }
        public sbyte HitCount { get; set; }
        public short Range { get; set; }
        public int BuffTime { get; set; }
        public short CostMp { get; set; }
        public short CostHp { get; set; }
        public short Damage { get; set; }
        public byte CriticalDamage { get; set; }
        public sbyte Mastery { get; set; }
        public int OptionalItemCost { get; set; }
        public int CostItem { get; set; }
        public short ItemCount { get; set; }
        public short CostBullet { get; set; }
        public short CostMeso { get; set; }
        public short ParameterA { get; set; }
        public short ParameterB { get; set; }
        public short ParameterC { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Strength { get; set; }
        public short WeaponAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicAttack { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short Probability { get; set; }
        public Point? Lt { get; private set; }
        public Point? Rb { get; private set; }
        public bool HasBuff => BuffTime > 0;

        public byte CurrentLevel
        {
            get => _currentLevel;
            set
            {
                _currentLevel = value;

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
            get => _maxLevel;
            set
            {
                _maxLevel = value;

                if (Parent != null && Character.IsInitialized)
                {
                    Update();
                }
            }
        }

        public SkillReference CachedReference => DataProvider.Skills.Data[MapleId][CurrentLevel];

        public Character Character => Parent.Parent;

        private bool Assigned { get; set; }

        public Skill(int mapleId, DateTime? expiration = null)
        {
            MapleId = mapleId;
            CurrentLevel = 0;
            MaxLevel = (byte)DataProvider.Skills.Data[MapleId].Count;
            Expiration = expiration ?? DateConstants.Permanent;
        }

        public Skill(int mapleId, byte currentLevel, byte maxLevel, DateTime? expiration = null)
        {
            MapleId = mapleId;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            Expiration = expiration ?? DateConstants.Permanent;
        }


        public Skill(SkillEntity entity)
        {
            Id = entity.Id;
            Assigned = true;
            MapleId = entity.SkillId;
            CurrentLevel = entity.Level;
            MaxLevel = entity.MasterLevel;
            Expiration = entity.Expiration;
        }

        public void Save()
        {
            using var dbContext = new MapleDbContext();
            var item = dbContext.Skills.Find(Id);
            var isNew = item == null;

            if (isNew)
            {
                item = new SkillEntity();
                dbContext.Skills.Add(item);
            }

            item.CharacterId = Character.Id;
            item.SkillId = MapleId;
            item.Level = CurrentLevel;
            item.MasterLevel = MaxLevel;
            item.Expiration = Expiration;

            dbContext.SaveChanges();

            if (isNew)
            {
                Id = item.Id;
            }
        }

        public void Delete()
        {
            using var dbContext = new MapleDbContext();
            var skill = dbContext.Skills.Find(Id);

            if (skill != null)
            {
                dbContext.Skills.Remove(skill);
                dbContext.SaveChanges();
            }

            Assigned = false;
        }

        public void Update()
        {
            using var pw = new PacketWriter(ServerOperationCode.SkillsAddPoint);
            pw.WriteByte(1);
            pw.WriteShort(1);
            pw.WriteInt(MapleId);
            pw.WriteInt(CurrentLevel);
            pw.WriteInt(MaxLevel);
            pw.WriteDateTime(Expiration);
            pw.WriteByte(4);
            Character.Send(pw);
        }

        public void Recalculate()
        {
            MobCount = CachedReference.MobCount;
            HitCount = CachedReference.HitCount;
            Range = CachedReference.Range;
            BuffTime = CachedReference.BuffTime;
            CostMp = CachedReference.CostMp;
            CostHp = CachedReference.CostHp;
            Damage = CachedReference.Damage;
            CriticalDamage = CachedReference.CriticalDamage;
            Mastery = CachedReference.Mastery;
            OptionalItemCost = CachedReference.OptionalItemCost;
            CostItem = CachedReference.CostItem;
            ItemCount = CachedReference.ItemCount;
            CostBullet = CachedReference.CostBullet;
            CostMeso = CachedReference.CostMeso;
            ParameterA = CachedReference.ParameterA;
            ParameterB = CachedReference.ParameterB;
            ParameterC = CachedReference.ParameterC;
            Speed = CachedReference.Speed;
            Jump = CachedReference.Jump;
            Strength = CachedReference.Strength;
            WeaponAttack = CachedReference.WeaponAttack;
            WeaponDefense = CachedReference.WeaponDefense;
            MagicAttack = CachedReference.MagicAttack;
            MagicDefense = CachedReference.MagicDefense;
            Accuracy = CachedReference.Accuracy;
            Avoidability = CachedReference.Avoidability;
            Hp = CachedReference.Hp;
            Mp = CachedReference.Mp;
            Probability = CachedReference.Probability;
            Lt = CachedReference.Lt;
            Rb = CachedReference.Rb;
        }

        public void Cast() => ApplyCosts();

        private void ApplyCosts()
        {
            switch (MapleId)
            {
                case (int) SkillNames.DragonKnight.DragonRoar:
                {
                    var lefthp = (int) (Character.PrimaryStats.MaxHealth * (ParameterA / 100.0d));
                    Character.PrimaryStats.Health -= (short) lefthp;
                    break;
                }

                case (int) SkillNames.Spearman.HyperBody when Character.PrimaryStats.HasBuff(MapleId):
                    // Already buffed
                    return;
                case (int) SkillNames.Spearman.HyperBody:
                {
                    var lefthp = (int) (Character.PrimaryStats.MaxHealth * (ParameterA / 100.0d));
                    Character.PrimaryStats.BuffBonuses.MaxHealth = (short) lefthp;
                    Character.PrimaryStats.MaxHealth += (short) lefthp;
                    lefthp = (int) (Character.PrimaryStats.MaxMana * (ParameterB / 100.0d));
                    Character.PrimaryStats.BuffBonuses.MaxMana = (short) lefthp;
                    Character.PrimaryStats.MaxMana += (short) lefthp;
                    Character.PrimaryStats.MaxMana = Character.PrimaryStats.TotalMaxMana;
                    Character.PrimaryStats.MaxHealth = Character.PrimaryStats.TotalMaxHealth;
                    break;
                }
            }


            if (CostMp > 0)
            {
                Character.PrimaryStats.Mana -= CostMp;
            }

            if (CostHp > 0)
            {
                Character.PrimaryStats.Health -= CostHp;
            }

            if (CostItem > 0)
            {
                Character.Items.Remove(CostItem, ItemCount);
            }

            if (CostMeso > 0)
            {
                var min = (short) (CostMeso - (80 + CurrentLevel * 5));
                var max = (short) (CostMeso + 80 + CurrentLevel * 5);
                var realAmount = (short) Functions.Random(min, max);
                if (Character.PrimaryStats.Meso - realAmount >= 0)
                {
                    Character.PrimaryStats.Meso -= realAmount;
                }
                else
                {
                    Character.LogCheatWarning(CheatType.InvalidSkillChange);
                }
            }
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteInt(MapleId);
            pw.WriteInt(CurrentLevel);
            return pw.ToArray();
        }
    }
}
