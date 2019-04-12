using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Buffs;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Items;
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

        public CharacterStats(Character character)
        {
            Parent = character;
            BuffDragonBlood = new BuffStatDragonBlood(BuffValueTypes.DragonBlood, Parent);
        }

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
        public short WeaponAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicAttack { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Agility { get; set; }


        public int Craft => Dexterity + Luck + Intelligence;

        public int TotalStr => Strength + EquipBonuses.Strength;
        public int TotalDex => Dexterity + EquipBonuses.Dexterity;
        public int TotalInt => Intelligence + EquipBonuses.Intelligence;
        public int TotalLuck => Luck + EquipBonuses.Luck;
        public short TotalMaxHealth => (short)(MaxHealth + EquipBonuses.MaxHealth + BuffBonuses.MaxHealth);
        public short TotalMaxMana => (short)(MaxMana + EquipBonuses.MaxMana + BuffBonuses.MaxMana);

        public int Acc
        {
            get
            {
                var acc = 0;

                if (IsBaseJob(Job.Bowman) || IsBaseJob(Job.Thief))
                {
                    acc = (int)((Luck * 0.3) + (Dexterity * 0.6));
                }
                else
                {
                    acc = (int)((Luck * 0.5) + (Dexterity * 0.8));
                }

                var buff = Parent.Skills[(int)SkillNames.Archer.BlessingOfAmazon];
                if (buff != null)
                {
                    acc += buff.ParameterA;
                }

                buff = Parent.Skills[(int)SkillNames.Rogue.NimbleBody];
                if (buff != null)
                {
                    acc += buff.ParameterA;
                }

                // TODO: Weapon mastery buff
                /*
                buff = Char.Skills.GetSkillLevelData(Char.Skills.GetMastery(), out byte lvl3);
                if (buff != null)
                {
                    acc += buff.Accurancy;
                }
                */

                return Math.Max(0, Math.Min(acc, 999));
            }
        }


        public short TotalMagicAttack => (short)Math.Max(0,
            Math.Min(MagicAttack + EquipBonuses.MagicAttack + BuffBonuses.MagicAttack, 1999));

        public short TotalMagicDefense => (short)Math.Max(0,
            Math.Min(MagicDefense + EquipBonuses.MagicDefense + BuffBonuses.MagicDefense, 1999));

        public short TotalWeaponAttack =>
            (short)Math.Max(0, Math.Min(EquipBonuses.WeaponAttack + BuffBonuses.WeaponAttack, 1999));

        public short TotalWeaponDefense =>
            (short)Math.Max(0, Math.Min(EquipBonuses.WeaponDefense + BuffBonuses.WeaponDefense, 1999));

        public short TotalAccuracy =>
            (short)Math.Max(0, Math.Min(Accuracy + EquipBonuses.Accuracy + BuffBonuses.Accuracy, 999));

        public short TotalAvoidability => (short)Math.Max(0,
            Math.Min(Avoidability + EquipBonuses.Avoidability + BuffBonuses.Avoidability, 999));

        public short TotalAgility =>
            (short)Math.Max(0, Math.Min(Agility + EquipBonuses.Agility + BuffBonuses.Agility, 999));

        public short TotalJump => (short)Math.Max(100, Math.Min(EquipBonuses.Jump + BuffBonuses.Jump, 123));
        public byte TotalSpeed => (byte)Math.Max(100, Math.Min(EquipBonuses.Speed + BuffBonuses.Speed, 200));

        private Dictionary<byte, EquipBonus> EquipStats { get; } = new Dictionary<byte, EquipBonus>();
        public StatBonus BuffBonuses { get; set; } = new StatBonus();
        public StatBonus EquipBonuses { get; set; } = new StatBonus();


        public BuffStat BuffWeaponAttack { get; } = new BuffStat(BuffValueTypes.WeaponAttack);
        public BuffStat BuffWeaponDefense { get; } = new BuffStat(BuffValueTypes.WeaponDefense);
        public BuffStat BuffMagicAttack { get; } = new BuffStat(BuffValueTypes.MagicAttack);
        public BuffStat BuffMagicDefense { get; } = new BuffStat(BuffValueTypes.MagicDefense);
        public BuffStat BuffAccuracy { get; } = new BuffStat(BuffValueTypes.Accuracy);
        public BuffStat BuffAvoidability { get; } = new BuffStat(BuffValueTypes.Avoidability);
        public BuffStat BuffHands { get; } = new BuffStat(BuffValueTypes.Hands);
        public BuffStat BuffSpeed { get; } = new BuffStat(BuffValueTypes.Speed);
        public BuffStat BuffJump { get; } = new BuffStat(BuffValueTypes.Jump);
        public BuffStat BuffMagicGuard { get; } = new BuffStat(BuffValueTypes.MagicGuard);
        public BuffStat BuffDarkSight { get; } = new BuffStat(BuffValueTypes.DarkSight);
        public BuffStat BuffBooster { get; } = new BuffStat(BuffValueTypes.Booster);
        public BuffStat BuffPowerGuard { get; } = new BuffStat(BuffValueTypes.PowerGuard);
        public BuffStat BuffMaxHealth { get; } = new BuffStat(BuffValueTypes.MaxHealth);
        public BuffStat BuffMaxMana { get; } = new BuffStat(BuffValueTypes.MaxMana);
        public BuffStat BuffInvincible { get; } = new BuffStat(BuffValueTypes.Invincible);
        public BuffStat BuffSoulArrow { get; } = new BuffStat(BuffValueTypes.SoulArrow);
        public BuffStat BuffStun { get; } = new BuffStat(BuffValueTypes.Stun);
        public BuffStat BuffPoison { get; } = new BuffStat(BuffValueTypes.Poison);
        public BuffStat BuffSeal { get; } = new BuffStat(BuffValueTypes.Seal);
        public BuffStat BuffDarkness { get; } = new BuffStat(BuffValueTypes.Darkness);
        public BuffStatComboAttack BuffComboAttack { get; } = new BuffStatComboAttack(BuffValueTypes.ComboAttack);
        public BuffStat BuffCharges { get; } = new BuffStat(BuffValueTypes.Charges);
        public BuffStatDragonBlood BuffDragonBlood { get; }
        public BuffStat BuffHolySymbol { get; } = new BuffStat(BuffValueTypes.HolySymbol);
        public BuffStat BuffMesoUp { get; } = new BuffStat(BuffValueTypes.MesoUp);
        public BuffStat BuffShadowPartner { get; } = new BuffStat(BuffValueTypes.ShadowPartner);
        public BuffStat BuffPickPocketMesoUp { get; } = new BuffStat(BuffValueTypes.PickPocketMesoUp);
        public BuffStatMesoGuard BuffMesoGuard { get; } = new BuffStatMesoGuard(BuffValueTypes.MesoGuard);
        public BuffStat BuffThaw { get; } = new BuffStat(BuffValueTypes.Thaw);
        public BuffStat BuffWeakness { get; } = new BuffStat(BuffValueTypes.Weakness);
        public BuffStat BuffCurse { get; } = new BuffStat(BuffValueTypes.Curse);


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

            var MaxHealth = (int)_maxHealth;
            var MaxMana = (int)_maxMana;

            if (Job == Job.Beginner)
            {
                MaxHealth += Functions.Random(12, 16);
                MaxMana += Functions.Random(10, 12);
            }
            else if (IsBaseJob(Job.Warrior))
            {
                MaxHealth += Functions.Random(24, 28);
                MaxMana += Functions.Random(4, 6);
            }
            else if (IsBaseJob(Job.Magician))
            {
                MaxHealth += Functions.Random(10, 14);
                MaxMana += Functions.Random(22, 24);
            }
            else if (IsBaseJob(Job.Bowman) || IsBaseJob(Job.Thief) || IsBaseJob(Job.Gm))
            {
                MaxHealth += Functions.Random(20, 24);
                MaxMana += Functions.Random(14, 16);
            }

            if (Parent.Skills.GetCurrentLevel((int)SkillNames.Swordsman.ImprovedMaxHpIncrease) > 0)
            {
                MaxHealth += Parent.Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease].ParameterA;
            }

            if (Parent.Skills.GetCurrentLevel((int)SkillNames.Magician.ImprovedMaxMpIncrease) > 0)
            {
                MaxMana += Parent.Skills[(int)SkillNames.Magician.ImprovedMaxMpIncrease].ParameterA;
            }

            MaxMana += Intelligence / 10;

            MaxHealth = Math.Min(30000, MaxHealth);
            MaxMana = Math.Min(30000, MaxMana);

            _maxHealth = (short)MaxHealth;
            _maxMana = (short)MaxMana;

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
            var pw = new PacketWriter(ServerOperationCode.StatsChanged);
            pw.WriteBool(true); // itemReaction

            var flag = statistics.Aggregate(0, (current, statistic) => current | (int)statistic);

            pw.WriteInt(flag);

            Array.Sort(statistics);

            foreach (var statistic in statistics)
            {
                switch (statistic)
                {
                    case StatisticType.Skin:
                        pw.WriteByte(Skin);
                        break;

                    case StatisticType.Face:
                        pw.WriteInt(Face);
                        break;

                    case StatisticType.Hair:
                        pw.WriteInt(Hair);
                        break;

                    case StatisticType.Level:
                        pw.WriteByte(Level);
                        break;

                    case StatisticType.Job:
                        pw.WriteShort((short)Job);
                        break;

                    case StatisticType.Strength:
                        pw.WriteShort(Strength);
                        break;

                    case StatisticType.Dexterity:
                        pw.WriteShort(Dexterity);
                        break;

                    case StatisticType.Intelligence:
                        pw.WriteShort(Intelligence);
                        break;

                    case StatisticType.Luck:
                        pw.WriteShort(Luck);
                        break;

                    case StatisticType.Health:
                        pw.WriteShort(Health);
                        break;

                    case StatisticType.MaxHealth:
                        pw.WriteShort(MaxHealth);
                        break;

                    case StatisticType.Mana:
                        pw.WriteShort(Mana);
                        break;

                    case StatisticType.MaxMana:
                        pw.WriteShort(MaxMana);
                        break;

                    case StatisticType.AbilityPoints:
                        pw.WriteShort(AbilityPoints);
                        break;

                    case StatisticType.SkillPoints:
                        pw.WriteShort(SkillPoints);
                        break;

                    case StatisticType.Experience:
                        pw.WriteInt(Experience);
                        break;

                    case StatisticType.Fame:
                        pw.WriteShort(Fame);
                        break;

                    case StatisticType.Mesos:
                        pw.WriteInt(Meso);
                        break;
                }
            }

            Parent.Send(pw);
        }

        public void UpdateApperance()
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerChangeEquips);
            pw.WriteInt(Parent.Id);
            pw.WriteBool(true);
            pw.WriteBytes(Parent.AppearanceToByteArray());
            pw.WriteByte(0);
            pw.WriteShort(0);
            Parent.Map.Send(pw, Parent);
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
                    var maxHealth = (int)MaxHealth;

                    if (maxHealth >= 30000)
                    {
                        return;
                    }

                    if (Job == Job.Beginner)
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Beginner - BaseHealthValues.Variation,
                            BaseHealthValues.Beginner);
                    }
                    else if (IsBaseJob(Job.Warrior))
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Warrior - BaseHealthValues.Variation,
                            BaseHealthValues.Warrior);
                        var skill = Parent.Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease];
                        maxHealth += skill?.ParameterB ?? 0;
                    }
                    else if (IsBaseJob(Job.Magician))
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Magician - BaseHealthValues.Variation,
                            BaseHealthValues.Magician);
                    }
                    else if (IsBaseJob(Job.Bowman))
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Bowman - BaseHealthValues.Variation,
                            BaseHealthValues.Bowman);
                    }
                    else if (IsBaseJob(Job.Thief))
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Thief - BaseHealthValues.Variation,
                            BaseHealthValues.Thief);
                    }
                    else if (IsBaseJob(Job.Gm))
                    {
                        maxHealth += Functions.Random(BaseHealthValues.Gm - BaseHealthValues.Variation,
                            BaseHealthValues.Gm);
                    }

                    maxHealth = Math.Min(30000, maxHealth);
                    MaxHealth = (short)maxHealth;
                    break;

                case StatisticType.MaxMana:
                    var maxMana = (int)MaxMana;

                    if (maxMana >= 30000)
                    {
                        return;
                    }

                    if (Job == Job.Beginner)
                    {
                        maxMana += Functions.Random(BaseManaValues.Beginner - BaseManaValues.Variation,
                            BaseManaValues.Beginner);
                    }
                    else if (IsBaseJob(Job.Warrior))
                    {
                        maxMana += Functions.Random(BaseManaValues.Warrior - BaseManaValues.Variation,
                            BaseManaValues.Warrior);
                    }
                    else if (IsBaseJob(Job.Magician))
                    {
                        maxMana += Functions.Random(BaseManaValues.Magician - BaseManaValues.Variation,
                            BaseManaValues.Magician);

                        var skill = Parent.Skills[(int)SkillNames.Magician.ImprovedMaxMpIncrease];
                        maxMana += skill?.ParameterB ?? 0;
                    }
                    else if (IsBaseJob(Job.Bowman))
                    {
                        maxMana += Functions.Random(BaseManaValues.Bowman - BaseManaValues.Variation,
                            BaseManaValues.Bowman);
                    }
                    else if (IsBaseJob(Job.Thief))
                    {
                        maxMana += Functions.Random(BaseManaValues.Thief - BaseManaValues.Variation,
                            BaseManaValues.Thief);
                    }
                    else if (IsBaseJob(Job.Gm))
                    {
                        maxMana += Functions.Random(BaseManaValues.Gm - BaseManaValues.Variation,
                            BaseManaValues.Gm);
                    }

                    maxMana = Math.Min(30000, maxMana);
                    MaxMana = (short)maxMana;
                    break;
                case StatisticType.None:
                case StatisticType.Skin:
                case StatisticType.Face:
                case StatisticType.Hair:
                case StatisticType.Level:
                case StatisticType.Job:
                case StatisticType.Health:
                case StatisticType.Mana:
                case StatisticType.AbilityPoints:
                case StatisticType.SkillPoints:
                case StatisticType.Experience:
                case StatisticType.Fame:
                case StatisticType.Mesos:
                case StatisticType.Pet:
                case StatisticType.GachaponExperience:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            AbilityPoints -= amount;
        }

        public int Avoidability
        {
            get
            {
                var eva = Luck / 2 + Dexterity / 4;

                var buff = Parent.Skills[4000000];
                if (buff != null)
                {
                    eva += buff.ParameterB;
                }

                return eva;
            }
        }

        public float SpeedMod => TotalSpeed + 100.0f;

        public short Mad => Intelligence;
        public short Mdd => Intelligence;

        private bool IsBaseJob(Job baseJob) => (int)Job / 100 * 100 == (int)baseJob;

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

        public bool HasBuff(int skillOrItemId)
        {
            var currentTime = DateTime.UtcNow;
            return
                BuffWeaponAttack.HasReferenceId(skillOrItemId, currentTime) ||
                BuffWeaponDefense.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMagicAttack.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMagicDefense.HasReferenceId(skillOrItemId, currentTime) ||
                BuffAccuracy.HasReferenceId(skillOrItemId, currentTime) ||
                BuffAvoidability.HasReferenceId(skillOrItemId, currentTime) ||
                BuffHands.HasReferenceId(skillOrItemId, currentTime) ||
                BuffSpeed.HasReferenceId(skillOrItemId, currentTime) ||
                BuffJump.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMagicGuard.HasReferenceId(skillOrItemId, currentTime) ||
                BuffDarkSight.HasReferenceId(skillOrItemId, currentTime) ||
                BuffBooster.HasReferenceId(skillOrItemId, currentTime) ||
                BuffPowerGuard.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMaxHealth.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMaxMana.HasReferenceId(skillOrItemId, currentTime) ||
                BuffInvincible.HasReferenceId(skillOrItemId, currentTime) ||
                BuffSoulArrow.HasReferenceId(skillOrItemId, currentTime) ||
                BuffStun.HasReferenceId(skillOrItemId, currentTime) ||
                BuffPoison.HasReferenceId(skillOrItemId, currentTime) ||
                BuffSeal.HasReferenceId(skillOrItemId, currentTime) ||
                BuffDarkness.HasReferenceId(skillOrItemId, currentTime) ||
                BuffComboAttack.HasReferenceId(skillOrItemId, currentTime) ||
                BuffCharges.HasReferenceId(skillOrItemId, currentTime) ||
                BuffDragonBlood.HasReferenceId(skillOrItemId, currentTime) ||
                BuffHolySymbol.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMesoUp.HasReferenceId(skillOrItemId, currentTime) ||
                BuffShadowPartner.HasReferenceId(skillOrItemId, currentTime) ||
                BuffPickPocketMesoUp.HasReferenceId(skillOrItemId, currentTime) ||
                BuffMesoGuard.HasReferenceId(skillOrItemId, currentTime) ||
                BuffThaw.HasReferenceId(skillOrItemId, currentTime) ||
                BuffWeakness.HasReferenceId(skillOrItemId, currentTime) ||
                BuffCurse.HasReferenceId(skillOrItemId, currentTime);
        }

        public void EncodeForLocal(PacketWriter packet, BuffValueTypes pSpecificFlag = BuffValueTypes.All)
        {
            var currentTime = DateTime.UtcNow;
            BuffValueTypes endFlag = 0;
            var buffPacket = new PacketWriter();
            BuffWeaponAttack.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffWeaponDefense.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMagicAttack.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMagicDefense.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffAccuracy.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffAvoidability.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffHands.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffSpeed.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffJump.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMagicGuard.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);

            // Do not activate it in hide
            if (!BuffDarkSight.HasReferenceId((int)SkillNames.Gm.Hide))
            {
                BuffDarkSight.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            }

            BuffBooster.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffPowerGuard.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMaxHealth.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMaxMana.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffInvincible.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffSoulArrow.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffStun.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffPoison.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffSeal.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffDarkness.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffComboAttack.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffCharges.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffDragonBlood.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffHolySymbol.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMesoUp.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffShadowPartner.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffPickPocketMesoUp.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffMesoGuard.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffThaw.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffWeakness.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            BuffCurse.EncodeForLocal(buffPacket, ref endFlag, currentTime, pSpecificFlag);
            packet.WriteUInt((uint)endFlag);
            packet.WriteBytes(buffPacket.ToArray());
        }

        public BuffValueTypes RemoveByReference(int pBuffValue, bool onlyReturn = false)
        {
            if (pBuffValue == 0)
            {
                return 0;
            }

            BuffValueTypes endFlag = 0;
            BuffWeaponAttack.TryResetByReference(pBuffValue, ref endFlag);
            BuffWeaponDefense.TryResetByReference(pBuffValue, ref endFlag);
            BuffMagicAttack.TryResetByReference(pBuffValue, ref endFlag);
            BuffMagicDefense.TryResetByReference(pBuffValue, ref endFlag);
            BuffAccuracy.TryResetByReference(pBuffValue, ref endFlag);
            BuffAvoidability.TryResetByReference(pBuffValue, ref endFlag);
            BuffHands.TryResetByReference(pBuffValue, ref endFlag);
            BuffSpeed.TryResetByReference(pBuffValue, ref endFlag);
            BuffJump.TryResetByReference(pBuffValue, ref endFlag);
            BuffMagicGuard.TryResetByReference(pBuffValue, ref endFlag);
            BuffDarkSight.TryResetByReference(pBuffValue, ref endFlag);
            BuffBooster.TryResetByReference(pBuffValue, ref endFlag);
            BuffPowerGuard.TryResetByReference(pBuffValue, ref endFlag);
            BuffMaxHealth.TryResetByReference(pBuffValue, ref endFlag);
            BuffMaxMana.TryResetByReference(pBuffValue, ref endFlag);
            BuffInvincible.TryResetByReference(pBuffValue, ref endFlag);
            BuffSoulArrow.TryResetByReference(pBuffValue, ref endFlag);
            BuffStun.TryResetByReference(pBuffValue, ref endFlag);
            BuffPoison.TryResetByReference(pBuffValue, ref endFlag);
            BuffSeal.TryResetByReference(pBuffValue, ref endFlag);
            BuffDarkness.TryResetByReference(pBuffValue, ref endFlag);
            BuffComboAttack.TryResetByReference(pBuffValue, ref endFlag);
            BuffCharges.TryResetByReference(pBuffValue, ref endFlag);
            BuffDragonBlood.TryResetByReference(pBuffValue, ref endFlag);
            BuffHolySymbol.TryResetByReference(pBuffValue, ref endFlag);
            BuffMesoUp.TryResetByReference(pBuffValue, ref endFlag);
            BuffShadowPartner.TryResetByReference(pBuffValue, ref endFlag);
            BuffPickPocketMesoUp.TryResetByReference(pBuffValue, ref endFlag);
            BuffMesoGuard.TryResetByReference(pBuffValue, ref endFlag);
            BuffThaw.TryResetByReference(pBuffValue, ref endFlag);
            BuffWeakness.TryResetByReference(pBuffValue, ref endFlag);
            BuffCurse.TryResetByReference(pBuffValue, ref endFlag);
            if (!onlyReturn)
            {
                Parent.Buffs.FinalizeDebuff(endFlag);
            }

            return endFlag;
        }

        public BuffValueTypes AllActiveBuffs()
        {
            var currentTime = DateTime.UtcNow;
            BuffValueTypes flags = 0;
            flags |= BuffWeaponAttack.GetState(currentTime);
            flags |= BuffWeaponDefense.GetState(currentTime);
            flags |= BuffMagicAttack.GetState(currentTime);
            flags |= BuffMagicDefense.GetState(currentTime);
            flags |= BuffAccuracy.GetState(currentTime);
            flags |= BuffAvoidability.GetState(currentTime);
            flags |= BuffHands.GetState(currentTime);
            flags |= BuffSpeed.GetState(currentTime);
            flags |= BuffJump.GetState(currentTime);
            flags |= BuffMagicGuard.GetState(currentTime);
            flags |= BuffDarkSight.GetState(currentTime);
            flags |= BuffBooster.GetState(currentTime);
            flags |= BuffPowerGuard.GetState(currentTime);
            flags |= BuffMaxHealth.GetState(currentTime);
            flags |= BuffMaxMana.GetState(currentTime);
            flags |= BuffInvincible.GetState(currentTime);
            flags |= BuffSoulArrow.GetState(currentTime);
            flags |= BuffStun.GetState(currentTime);
            flags |= BuffPoison.GetState(currentTime);
            flags |= BuffSeal.GetState(currentTime);
            flags |= BuffDarkness.GetState(currentTime);
            flags |= BuffComboAttack.GetState(currentTime);
            flags |= BuffCharges.GetState(currentTime);
            flags |= BuffDragonBlood.GetState(currentTime);
            flags |= BuffHolySymbol.GetState(currentTime);
            flags |= BuffMesoUp.GetState(currentTime);
            flags |= BuffShadowPartner.GetState(currentTime);
            flags |= BuffPickPocketMesoUp.GetState(currentTime);
            flags |= BuffMesoGuard.GetState(currentTime);
            flags |= BuffThaw.GetState(currentTime);
            flags |= BuffWeakness.GetState(currentTime);
            flags |= BuffCurse.GetState(currentTime);
            return flags;
        }

        public void CheckExpired(DateTime currentTime)
        {
            BuffValueTypes endFlag = 0;
            BuffWeaponAttack.TryReset(currentTime, ref endFlag);
            BuffWeaponDefense.TryReset(currentTime, ref endFlag);
            BuffMagicAttack.TryReset(currentTime, ref endFlag);
            BuffMagicDefense.TryReset(currentTime, ref endFlag);
            BuffAccuracy.TryReset(currentTime, ref endFlag);
            BuffAvoidability.TryReset(currentTime, ref endFlag);
            BuffHands.TryReset(currentTime, ref endFlag);
            BuffSpeed.TryReset(currentTime, ref endFlag);
            BuffJump.TryReset(currentTime, ref endFlag);
            BuffMagicGuard.TryReset(currentTime, ref endFlag);
            BuffDarkSight.TryReset(currentTime, ref endFlag);
            BuffBooster.TryReset(currentTime, ref endFlag);
            BuffPowerGuard.TryReset(currentTime, ref endFlag);
            if (BuffMaxHealth.TryReset(currentTime, ref endFlag) &&
                BuffMaxMana.TryReset(currentTime, ref endFlag))
            {
                Parent.Buffs.CancelHyperBody();
            }

            BuffInvincible.TryReset(currentTime, ref endFlag);
            BuffSoulArrow.TryReset(currentTime, ref endFlag);
            BuffStun.TryReset(currentTime, ref endFlag);
            BuffPoison.TryReset(currentTime, ref endFlag);
            BuffSeal.TryReset(currentTime, ref endFlag);
            BuffDarkness.TryReset(currentTime, ref endFlag);
            BuffComboAttack.TryReset(currentTime, ref endFlag);
            BuffCharges.TryReset(currentTime, ref endFlag);
            BuffDragonBlood.TryReset(currentTime, ref endFlag);
            BuffHolySymbol.TryReset(currentTime, ref endFlag);
            BuffMesoUp.TryReset(currentTime, ref endFlag);
            BuffShadowPartner.TryReset(currentTime, ref endFlag);
            BuffPickPocketMesoUp.TryReset(currentTime, ref endFlag);
            BuffMesoGuard.TryReset(currentTime, ref endFlag);
            BuffThaw.TryReset(currentTime, ref endFlag);
            BuffWeakness.TryReset(currentTime, ref endFlag);
            BuffCurse.TryReset(currentTime, ref endFlag);
            Parent.Buffs.FinalizeDebuff(endFlag);
        }

        public void Reset(bool sendPacket)
        {
            BuffValueTypes flags = 0;
            flags |= BuffWeaponAttack.Reset();
            flags |= BuffWeaponDefense.Reset();
            flags |= BuffMagicAttack.Reset();
            flags |= BuffMagicDefense.Reset();
            flags |= BuffAccuracy.Reset();
            flags |= BuffAvoidability.Reset();
            flags |= BuffHands.Reset();
            flags |= BuffSpeed.Reset();
            flags |= BuffJump.Reset();
            flags |= BuffMagicGuard.Reset();
            flags |= BuffDarkSight.Reset();
            flags |= BuffBooster.Reset();
            flags |= BuffPowerGuard.Reset();
            flags |= BuffMaxHealth.Reset();
            flags |= BuffMaxMana.Reset();
            flags |= BuffInvincible.Reset();
            flags |= BuffSoulArrow.Reset();
            flags |= BuffStun.Reset();
            flags |= BuffPoison.Reset();
            flags |= BuffSeal.Reset();
            flags |= BuffDarkness.Reset();
            flags |= BuffComboAttack.Reset();
            flags |= BuffCharges.Reset();
            flags |= BuffDragonBlood.Reset();
            flags |= BuffHolySymbol.Reset();
            flags |= BuffMesoUp.Reset();
            flags |= BuffShadowPartner.Reset();
            flags |= BuffPickPocketMesoUp.Reset();
            flags |= BuffMesoGuard.Reset();
            flags |= BuffThaw.Reset();
            flags |= BuffWeakness.Reset();
            flags |= BuffCurse.Reset();

            if (flags.HasFlag(BuffValueTypes.MaxHealth))
            {
                Parent.Buffs.CancelHyperBody();
            }

            Parent.Buffs.FinalizeDebuff(flags, sendPacket);
        }

        public void AddEquipStats(sbyte slot, Item equip, bool isLoading)
        {
            var realSlot = (byte)Math.Abs(slot);
            if (equip != null)
            {
                EquipBonus equipBonus;
                if (!EquipStats.TryGetValue(realSlot, out equipBonus))
                {
                    equipBonus = new EquipBonus();
                }

                equipBonus.MapleId = equip.MapleId;
                equipBonus.MaxHealth = equip.Health;
                equipBonus.MaxMana = equip.Mana;
                equipBonus.Strength = equip.Strength;
                equipBonus.Intelligence = equip.Intelligence;
                equipBonus.Dexterity = equip.Dexterity;
                equipBonus.Luck = equip.Luck;
                equipBonus.Speed = equip.Speed;
                equipBonus.WeaponAttack = equip.WeaponAttack;
                equipBonus.WeaponDefense = equip.WeaponDefense;
                equipBonus.MagicAttack = equip.MagicAttack;
                equipBonus.MagicDefense = equip.MagicDefense;
                equipBonus.Avoidability = equip.Avoidability;
                equipBonus.Accuracy = equip.Accuracy;
                equipBonus.Agility = equip.Agility;
                equipBonus.Jump = equip.Jump;
                EquipStats[realSlot] = equipBonus;
            }

            else
            {
                EquipStats.Remove(realSlot);
            }

            CalculateAdditions(true, isLoading);
        }

        public void CalculateAdditions(bool updateEquips, bool isLoading)
        {
            if (updateEquips)
            {
                EquipBonuses = new StatBonus();
                foreach (var data in EquipStats)
                {
                    var item = data.Value;
                    if (EquipBonuses.Dexterity + item.Dexterity > short.MaxValue)
                    {
                        EquipBonuses.Dexterity = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.Dexterity += item.Dexterity;
                    }

                    if (EquipBonuses.Intelligence + item.Intelligence > short.MaxValue)
                    {
                        EquipBonuses.Intelligence = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.Intelligence += item.Intelligence;
                    }

                    if (EquipBonuses.Luck + item.Luck > short.MaxValue)
                    {
                        EquipBonuses.Luck = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.Luck += item.Luck;
                    }

                    if (EquipBonuses.Strength + item.Strength > short.MaxValue)
                    {
                        EquipBonuses.Strength = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.Strength += item.Strength;
                    }

                    if (EquipBonuses.MaxMana + item.MaxMana > short.MaxValue)
                    {
                        EquipBonuses.MaxMana = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.MaxMana += item.MaxMana;
                    }

                    if (EquipBonuses.MaxHealth + item.MaxHealth > short.MaxValue)
                    {
                        EquipBonuses.MaxHealth = short.MaxValue;
                    }
                    else
                    {
                        EquipBonuses.MaxHealth += item.MaxHealth;
                    }

                    EquipBonuses.WeaponAttack += item.WeaponAttack;

// TODO: Shield mastery buff
                    if (data.Key == (byte)EquipSlot.Shield)
                    {
                    }

                    EquipBonuses.WeaponDefense += item.WeaponDefense;
                    EquipBonuses.MagicAttack += item.MagicAttack;
                    EquipBonuses.MagicDefense += item.MagicDefense;
                    EquipBonuses.Accuracy += item.Accuracy;
                    EquipBonuses.Avoidability += item.Avoidability;
                    EquipBonuses.Speed += item.Speed;
                    EquipBonuses.Jump += item.Jump;
                    EquipBonuses.Agility += item.Agility;
                    EquipBonuses.WeaponAttack = (short)Math.Max(0, Math.Min((int)EquipBonuses.WeaponAttack, 1999));
                    EquipBonuses.WeaponDefense = (short)Math.Max(0, Math.Min((int)EquipBonuses.WeaponDefense, 1999));
                    EquipBonuses.MagicAttack = (short)Math.Max(0, Math.Min((int)EquipBonuses.MagicAttack, 1999));
                    EquipBonuses.MagicDefense = (short)Math.Max(0, Math.Min((int)EquipBonuses.MagicDefense, 1999));
                    EquipBonuses.Accuracy = (short)Math.Max(0, Math.Min((int)EquipBonuses.Accuracy, 999));
                    EquipBonuses.Avoidability = (short)Math.Max(0, Math.Min((int)EquipBonuses.Avoidability, 999));
                    EquipBonuses.Agility = (short)Math.Max(0, Math.Min((int)EquipBonuses.Agility, 999));
                    EquipBonuses.Speed = (short)Math.Max(100, Math.Min((int)EquipBonuses.Speed, 200));
                    EquipBonuses.Jump = (short)Math.Max(100, Math.Min((int)EquipBonuses.Jump, 123));
                }
            }

            if (!isLoading)
            {
                CheckHpmp();
            }
        }

        public void CheckHpmp()
        {
            var mhp = GetMaxHealth(false);
            var mmp = GetMaxMana(false);
            if (Health > mhp)
            {
                Health = mhp;
            }

            if (Mana > mmp)
            {
                Mana = mmp;
            }
        }

        public void CheckBoosters()
        {
            var equippedId = Parent.Items[EquipmentSlot.Weapon];
            if (equippedId == null)
            {
                return;
            }

            BuffValueTypes removed = 0;
            var currentTime = DateTime.UtcNow;
            if (BuffBooster.IsActive(currentTime))
            {
                removed |= RemoveByReference(BuffBooster.ReferenceId, true);
            }

            if (BuffCharges.IsActive(currentTime))
            {
                removed |= RemoveByReference(BuffCharges.ReferenceId, true);
            }

            if (BuffComboAttack.IsActive(currentTime))
            {
                removed |= RemoveByReference(BuffComboAttack.ReferenceId, true);
            }

            if (BuffSoulArrow.IsActive(currentTime))
            {
                removed |= RemoveByReference(BuffSoulArrow.ReferenceId, true);
            }

            Parent.Buffs.FinalizeDebuff(removed);
        }

        public short GetTotalStr() { return (short)(BaseStrength + EquipBonuses.Strength); }
        public short GetTotalDex() { return (short)(BaseDexterity + EquipBonuses.Dexterity); }
        public short GetTotalInt() { return (short)(BaseIntelligence + EquipBonuses.Intelligence); }
        public short GetTotalLuck() { return (short)(Luck + EquipBonuses.Luck); }
        public short GetTotalMagicAttack() { return (short)(BaseIntelligence + EquipBonuses.MagicAttack); }
        public short GetTotalMagicDef() { return (short)(BaseIntelligence + EquipBonuses.MagicDefense); }

        public short GetStrAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((BaseStrength + EquipBonuses.Strength + BuffBonuses.Strength) > short.MaxValue
                    ? short.MaxValue
                    : (BaseStrength + EquipBonuses.Strength + BuffBonuses.Strength));
            }

            return BaseStrength;
        }

        public short GetDexAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((BaseDexterity + EquipBonuses.Dexterity + BuffBonuses.Dexterity) > short.MaxValue
                    ? short.MaxValue
                    : (BaseDexterity + EquipBonuses.Dexterity + BuffBonuses.Dexterity));
            }

            return BaseDexterity;
        }

        public short GetIntAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((BaseIntelligence + EquipBonuses.Intelligence + BuffBonuses.Intelligence) >
                               short.MaxValue
                    ? short.MaxValue
                    : (BaseIntelligence + EquipBonuses.Intelligence + BuffBonuses.Intelligence));
            }

            return BaseIntelligence;
        }

        public short GetLuckAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((Luck + EquipBonuses.Luck + BuffBonuses.Luck) > short.MaxValue
                    ? short.MaxValue
                    : (Luck + EquipBonuses.Luck + BuffBonuses.Luck));
            }

            return Luck;
        }

        public short GetMaxHealth(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((MaxHealth + EquipBonuses.MaxHealth + BuffBonuses.MaxHealth) > short.MaxValue
                    ? short.MaxValue
                    : (MaxHealth + EquipBonuses.MaxHealth + BuffBonuses.MaxHealth));
            }

            return MaxHealth;
        }

        public short GetMaxMana(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((MaxMana + EquipBonuses.MaxMana + BuffBonuses.MaxMana) > short.MaxValue
                    ? short.MaxValue
                    : (MaxMana + EquipBonuses.MaxMana + BuffBonuses.MaxMana));
            }

            return MaxMana;
        }
    }
}
