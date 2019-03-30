using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterStats
    {
        public Character Parent { get; set; }

        
        private byte _skin;
        private int _face;
        private int _hair;
        private byte _level;
        private Job _job;
        private short _strength;
        private short _dexterity;
        private short _intelligence;
        private short _luck;
        private short _health;
        private short _maxHealth;
        private short _mana;
        private short _maxMana;
        private short _abilityPoints;
        private short _skillPoints;
        private int _experience;
        private short _fame;
        private int _meso;
        
        public short BaseStrength { get; set; }
        public short BaseDexterity { get; set; }
        public short BaseIntelligence { get; set; }
        public short BaseLuck { get; set; }
        public short BaseHealth { get; set; }
        public short BaseMaxHealth { get; set; }
        public short BaseMana { get; set; }
        public short BaseMaxMana { get; set; }
        public int BuddyListSlots { get; set; } = 20;
        public Gender Gender { get; set; }

        public byte BaseSpeed { get; set; }
        public float SpeedMode { get; set; }
        public byte BaseJump { get; set; }
        public float JumpMode { get; set; }

        public short AdjustedStrength => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedDexterity => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedIntelligence => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedLuck => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedHealth => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedMaxHealth => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedMana => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short AdjustedMaxMana => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);

        public StatBonus BuffBonuses { get; set; } = new StatBonus();
        public StatBonus ItemBonuses { get; set; } = new StatBonus();


      public byte Skin
        {
            get => _skin;
            set
            {
                if (!DataProvider.Styles.Skins.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                _skin = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Skin);
                    UpdateApperance();
                }
            }
        }

        public int Face
        {
            get => _face;
            set
            {
                if (Gender == Gender.Male && !DataProvider.Styles.MaleFaces.Contains(value) ||
                    Gender == Gender.Female && !DataProvider.Styles.FemaleFaces.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                _face = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Face);
                    UpdateApperance();
                }
            }
        }

        public int Hair
        {
            get => _hair;
            set
            {
                if (Gender == Gender.Male && !DataProvider.Styles.MaleHairs.Contains(value) ||
                    Gender == Gender.Female && !DataProvider.Styles.FemaleHairs.Contains(value))
                {
                    //throw new StyleUnavailableException();
                }

                _hair = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Hair);
                    UpdateApperance();
                }
            }
        }

        public int HairStyleOffset => Hair / 10 * 10;

        public int FaceStyleOffset => Face - 10 * (Face / 10) + (Gender == Gender.Male ? 20000 : 21000);

        public int HairColorOffset => Hair - 10 * (Hair / 10);

        public int FaceColorOffset => (Face / 100 - 10 * (Face / 1000)) * 100;

        public byte Level
        {
            get => _level;
            set
            {
                value = Math.Min(value, (byte)200);
                var delta = value - Level;

                if (!Parent.IsInitialized)
                {
                    _level = value;
                }
                else
                {
                    if (delta < 0)
                    {
                        _level = value;

                        Update(StatisticType.Level);
                    }
                    else
                    {
                        for (var i = 0; i < delta; i++)
                        {
                            LevelUp();
                        }

                        Health = MaxHealth;
                        Mana = MaxMana;
                    }

                    UpdateStatsForParty();
                }
            }
        }

        private void LevelUp()
        {
            _level++;

            _abilityPoints += 5;
            _skillPoints += 3;

            var maxHp = (int)_maxHealth;
            var maxMp = (int)_maxMana;

            if (Job == Job.Beginner)
            {
                maxHp += Functions.Random(12, 16);
                maxMp += Functions.Random(10, 12);
            }
            else if (IsBaseJob(Job.Warrior))
            {
                maxHp += Functions.Random(24, 28);
                maxMp += Functions.Random(4, 6);
            }
            else if (IsBaseJob(Job.Magician))
            {
                maxHp += Functions.Random(10, 14);
                maxMp += Functions.Random(22, 24);
            }
            else if (IsBaseJob(Job.Bowman) || IsBaseJob(Job.Thief) || IsBaseJob(Job.Gm))
            {
                maxHp += Functions.Random(20, 24);
                maxMp += Functions.Random(14, 16);
            }

            if (Parent.Skills.GetCurrentLevel((int)SkillNames.Swordsman.ImprovedMaxHpIncrease) > 0)
            {
                maxHp += Parent.Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease].ParameterA;
            }

            if (Parent.Skills.GetCurrentLevel((int)SkillNames.Magician.ImprovedMaxMpIncrease) > 0)
            {
                maxMp += Parent.Skills[(int)SkillNames.Magician.ImprovedMaxMpIncrease].ParameterA;
            }

            maxMp += Intelligence / 10;

            maxHp = Math.Min(30000, maxHp);
            maxMp = Math.Min(30000, maxMp);

            _maxHealth = (short)maxHp;
            _maxMana = (short)maxMp;

            Update(StatisticType.Level, StatisticType.MaxHealth, StatisticType.MaxMana, StatisticType.AbilityPoints,
                StatisticType.SkillPoints);
            Parent.ShowRemoteUserEffect(UserEffect.LevelUp);
        }

        public Job Job
        {
            get => _job;
            set
            {
                _job = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Job);
                    UpdateStatsForParty();
                    Parent.ShowRemoteUserEffect(UserEffect.JobChanged);
                }
            }
        }

        public short Strength
        {
            get => _strength;
            set
            {
                _strength = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Strength);
                }
            }
        }

        public short Dexterity
        {
            get => _dexterity;
            set
            {
                _dexterity = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Dexterity);
                }
            }
        }

        public short Intelligence
        {
            get => _intelligence;
            set
            {
                _intelligence = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Intelligence);
                }
            }
        }

        public short Luck
        {
            get => _luck;
            set
            {
                _luck = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Luck);
                }
            }
        }

        public short Health
        {
            get => _health;
            set
            {
                if (value < 0)
                {
                    _health = 0;
                }
                else if (value > MaxHealth)
                {
                    _health = MaxHealth;
                }
                else
                {
                    _health = value;
                }

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Health);
                }
            }
        }

        public short MaxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.MaxHealth);
                }
            }
        }

        public short Mana
        {
            get => _mana;
            set
            {
                _mana = value;
                _mana = Math.Max(_mana, (short)0);
                _mana = Math.Min(_mana, (short)30000);
                _mana = Math.Min(_mana, _maxMana);

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Mana);
                }
            }
        }

        public short MaxMana
        {
            get => _maxMana;
            set
            {
                _maxMana = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.MaxMana);
                }
            }
        }

        public short AbilityPoints
        {
            get => _abilityPoints;
            set
            {
                _abilityPoints = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.AbilityPoints);
                }
            }
        }

        public short SkillPoints
        {
            get => _skillPoints;
            set
            {
                _skillPoints = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.SkillPoints);
                }
            }
        }

        public int Experience
        {
            get => _experience;
            set
            {
                var delta = value - _experience;

                _experience = value;

                if (ServerConfig.Instance.EnableMultiLeveling)
                {
                    while (_experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        _experience -= ExperienceTables.CharacterLevel[Level];

                        Level++;
                    }
                }
                else
                {
                    if (_experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        _experience -= ExperienceTables.CharacterLevel[Level];

                        Level++;
                    }

                    if (_experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        _experience = ExperienceTables.CharacterLevel[Level] - 1;
                    }
                }

                if (Parent.IsInitialized && delta != 0)
                {
                    Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseExp, amount: delta, isWhite: true));
                    Update(StatisticType.Experience);
                }
            }
        }

        public short Fame
        {
            get => _fame;
            set
            {
                _fame = value;

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Fame);
                }
            }
        }

        public int Meso
        {
            get => _meso;
            set
            {
                _meso = value;
                _meso = Math.Max(_meso, 0);

                if (Parent.IsInitialized)
                {
                    Update(StatisticType.Mesos);
                }
            }
        }
        
         public void Update(params StatisticType[] statistics)
        {
            var oPacket = new PacketWriter(ServerOperationCode.StatsChanged);
            oPacket.WriteBool(true); // itemReaction

            var flag = statistics.Aggregate(0, (current, statistic) => current | (int)statistic);

            oPacket.WriteInt(flag);

            Array.Sort(statistics);

            foreach (var statistic in statistics)
            {
                switch (statistic)
                {
                    case StatisticType.Skin:
                        oPacket.WriteByte(Skin);
                        break;

                    case StatisticType.Face:
                        oPacket.WriteInt(Face);
                        break;

                    case StatisticType.Hair:
                        oPacket.WriteInt(Hair);
                        break;

                    case StatisticType.Level:
                        oPacket.WriteByte(Level);
                        break;

                    case StatisticType.Job:
                        oPacket.WriteShort((short)Job);
                        break;

                    case StatisticType.Strength:
                        oPacket.WriteShort(Strength);
                        break;

                    case StatisticType.Dexterity:
                        oPacket.WriteShort(Dexterity);
                        break;

                    case StatisticType.Intelligence:
                        oPacket.WriteShort(Intelligence);
                        break;

                    case StatisticType.Luck:
                        oPacket.WriteShort(Luck);
                        break;

                    case StatisticType.Health:
                        oPacket.WriteShort(Health);
                        break;

                    case StatisticType.MaxHealth:
                        oPacket.WriteShort(MaxHealth);
                        break;

                    case StatisticType.Mana:
                        oPacket.WriteShort(Mana);
                        break;

                    case StatisticType.MaxMana:
                        oPacket.WriteShort(MaxMana);
                        break;

                    case StatisticType.AbilityPoints:
                        oPacket.WriteShort(AbilityPoints);
                        break;

                    case StatisticType.SkillPoints:
                        oPacket.WriteShort(SkillPoints);
                        break;

                    case StatisticType.Experience:
                        oPacket.WriteInt(Experience);
                        break;

                    case StatisticType.Fame:
                        oPacket.WriteShort(Fame);
                        break;

                    case StatisticType.Mesos:
                        oPacket.WriteInt(Meso);
                        break;
                }
            }

            Parent.Send(oPacket);
        }
         
         public void UpdateApperance()
         {
             using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerChangeEquips))
             {
                 oPacket.WriteInt(Parent.Id);
                 oPacket.WriteBool(true);
                 oPacket.WriteBytes(Parent.AppearanceToByteArray());
                 oPacket.WriteByte(0);
                 oPacket.WriteShort(0);
                 Parent.Map.Send(oPacket, Parent);
             }
         }
         
         public void AddAbility(StatisticType statistic, short mod, bool isReset)
         {
             var maxStat = short.MaxValue;

             lock (this)
             {
                 switch (statistic)
                 {
                     case StatisticType.Strength:
                         if (Strength >= maxStat)
                         {
                             return;
                         }

                         Strength += mod;
                         break;

                     case StatisticType.Dexterity:
                         if (Dexterity >= maxStat)
                         {
                             return;
                         }

                         Dexterity += mod;
                         break;

                     case StatisticType.Intelligence:
                         if (Intelligence >= maxStat)
                         {
                             return;
                         }

                         Intelligence += mod;
                         break;

                     case StatisticType.Luck:
                         if (Luck >= maxStat)
                         {
                             return;
                         }

                         Luck += mod;
                         break;

                     case StatisticType.MaxHealth:
                         if (MaxHealth >= 30000)
                         {
                             return;
                         }

                         MaxHealth += mod;
                         break;
                     case StatisticType.MaxMana:
                         if (MaxMana >= 30000)
                         {
                             return;
                         }

                         MaxMana += mod;
                         break;
                 }

                 if (!isReset)
                 {
                     AbilityPoints -= mod;
                 }
             }
         }
         
           public void DistributeAp(StatisticType type, short amount = 1)
        {
            switch (type)
            {
                case StatisticType.Strength:
                    Strength += amount;
                    break;

                case StatisticType.Dexterity:
                    Dexterity += amount;
                    break;

                case StatisticType.Intelligence:
                    Intelligence += amount;
                    break;

                case StatisticType.Luck:
                    Luck += amount;
                    break;

                case StatisticType.MaxHealth:
                    var maxHp = (int)MaxHealth;

                    if (MaxHealth >= 30000)
                    {
                        return;
                    }

                    if (Job == Job.Beginner)
                    {
                        maxHp += Functions.Random(8, 12);
                    }
                    else if (IsBaseJob(Job.Warrior))
                    {
                        maxHp += Functions.Random(20, 24);
                        var skill = Parent.Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease];
                        maxHp += skill?.ParameterB ?? 0;
                    }
                    else if (IsBaseJob(Job.Magician))
                    {
                        maxHp += Functions.Random(6, 10);
                    }
                    else if (IsBaseJob(Job.Bowman))
                    {
                        maxHp += Functions.Random(16, 20);
                    }
                    else if (IsBaseJob(Job.Thief))
                    {
                        maxHp += Functions.Random(20, 24);
                    }

                    maxHp = Math.Min(30000, maxHp);
                    MaxHealth = (short)maxHp;
                    break;

                case StatisticType.MaxMana:
                    var maxMp = (int)MaxMana;

                    if (MaxMana >= 30000)
                    {
                        return;
                    }

                    if (Job == Job.Beginner)
                    {
                        maxMp += Functions.Random(6, 8);
                    }
                    else if (IsBaseJob(Job.Warrior))
                    {
                        maxMp += Functions.Random(2, 4);
                    }
                    else if (IsBaseJob(Job.Magician))
                    {
                        maxMp += Functions.Random(18, 20);

                        var skill = Parent.Skills[(int)SkillNames.Magician.ImprovedMaxMpIncrease];
                        maxMp += skill?.ParameterB ?? 0;
                    }
                    else if (IsBaseJob(Job.Bowman))
                    {
                        maxMp += Functions.Random(10, 12);
                    }
                    else if (IsBaseJob(Job.Thief))
                    {
                        maxMp += Functions.Random(10, 12);
                    }


                    maxMp = Math.Min(30000, maxMp);
                    MaxMana = (short)maxMp;
                    break;
            }

            AbilityPoints -= amount;
        }

        private bool IsBaseJob(Job baseJob)
        {
            var currentBaseJob = (int)Job / 100 * 100;
            return currentBaseJob == (int)baseJob;
        }

        public void Load(CharacterEntity character)
        {
            _abilityPoints = character.AbilityPoints;
            _dexterity = character.Dexterity;
            _experience = character.Experience;
            _face = character.Face;
            _fame = character.Fame;
            _hair = character.Hair;
            _health = character.Health;
            _intelligence = character.Intelligence;
            _job = (Job)character.Job;
            _level = character.Level;
            _luck = character.Luck;
            _maxHealth = character.MaxHealth;
            _maxMana = character.MaxMana;
            _meso = character.Meso;
            _mana = character.Mana;
            _skin = character.Skin;
            _strength = character.Strength;
            _skillPoints = character.SkillPoints;
            _strength = character.Strength;
            BuddyListSlots = character.BuddyListSlots;
            Gender = (Gender)character.Gender;
        }
        
        
        public void UpdateStatsForParty()
        {
            // TODO
        }
    }
}
