using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Server;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Shops;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Game.Maple.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public partial class Character : MapObject, IMoveable, ISpawnable, IMapleSavable
    {
        public GameClient Client { get; }
        public int Id { get; set; }
        public int AccountId { get; set; }
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; private set; }
        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int? GuildRank { get; set; }
        public int BuddyListSlots { get; private set; } = 20;
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }
        public CharacterItems Items { get; }
        public CharacterSkills Skills { get; }
        public CharacterQuests Quests { get; }
        public CharacterBuffs Buffs { get; }
        public CharacterKeymap Keymap { get; }
        public CharacterTrocks Trocks { get; }
        public CharacterMemos Memos { get; }
        public CharacterStorage Storage { get; }
        public ControlledMobs ControlledMobs { get; }
        public ControlledNpcs ControlledNpcs { get; }
        public Trade Trade { get; set; }
        public PlayerShop PlayerShop { get; set; }
        public CharacterGuild Guild { get; set; }
        public CharacterParty Party { get; set; }
        public CharacterSkillMacros SkillMacros { get; set; }
        public ANpcScript NpcScript { get; set; }
        public Shop CurrentNpcShop { get; set; }
        public CharacterDamage Damage { get; set; }
        private bool Assigned { get; set; }

        public DateTime LastHealthHealOverTime { get; set; } = new DateTime();
        public DateTime LastManaHealOverTime { get; set; } = new DateTime();

        private Gender _gender;
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
        private string _chalkboard;
        private int _itemEffect;

        private readonly ILogger _log = LogManager.CreateLogger<Character>();

        public Gender Gender
        {
            get => _gender;
            set
            {
                _gender = value;

                if (IsInitialized)
                {
                    var oPacket = new PacketWriter(ServerOperationCode.SetGender);
                    oPacket.WriteByte((byte)Gender);
                    oPacket.WriteByte(1);
                    Client.Send(oPacket);
                }
            }
        }

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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (!IsInitialized)
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

            if (Job == Job.Beginner)
            {
                var totalUsed = Skills.GetCurrentLevel(1000) + Skills.GetCurrentLevel(1001) + Skills.GetCurrentLevel(1002);
                if (totalUsed < 6)
                {
                    _skillPoints += 1;
                }
            }
            else
            {
                _skillPoints += 3;
            }

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

            if (Skills.GetCurrentLevel((int)SkillNames.Swordsman.ImprovedMaxHpIncrease) > 0)
            {
                maxHp += Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease].ParameterA;
            }

            if (Skills.GetCurrentLevel((int)SkillNames.Magician.ImprovedMaxMpIncrease) > 0)
            {
                maxMp += Skills[2000001].ParameterA;
            }

            maxMp += Intelligence / 10;

            maxHp = Math.Min(30000, maxHp);
            maxMp = Math.Min(30000, maxMp);

            _maxHealth = (short)maxHp;
            _maxMana = (short)maxMp;

            Update(StatisticType.Level, StatisticType.MaxHealth, StatisticType.MaxMana, StatisticType.AbilityPoints, StatisticType.SkillPoints);
            ShowRemoteUserEffect(UserEffect.LevelUp);
        }

        public Job Job
        {
            get => _job;
            set
            {
                _job = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Job);
                    UpdateStatsForParty();
                    ShowRemoteUserEffect(UserEffect.JobChanged);
                }
            }
        }

        public short Strength
        {
            get => _strength;
            set
            {
                _strength = value;

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized)
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

                if (IsInitialized && delta != 0)
                {
                    Client.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseExp, amount: delta, isWhite: true));
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

                if (IsInitialized)
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

                if (IsInitialized)
                {
                    Update(StatisticType.Mesos);
                }
            }
        }

        public bool IsAlive => Health > 0;

        public bool IsMaster => Client.Account?.IsMaster ?? false;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => Level >= 30;

        public QuestReference LastQuest { get; set; }

        public string Chalkboard
        {
            get => _chalkboard;
            set
            {
                _chalkboard = value;

                using (var oPacket = new PacketWriter(ServerOperationCode.Chalkboard))
                {
                    oPacket.WriteInt(Id);
                    oPacket.WriteBool(!string.IsNullOrEmpty(Chalkboard));
                    oPacket.WriteString(Chalkboard);
                    Map.Send(oPacket);
                }
            }
        }

        public int ItemEffect
        {
            get => _itemEffect;
            set
            {
                _itemEffect = value;
                using (var oPacket = new PacketWriter(ServerOperationCode.ItemEffect))
                {
                    oPacket.WriteInt(Id);
                    oPacket.WriteInt(_itemEffect);
                    Map.Send(oPacket, this);
                }
            }
        }

        public Portal ClosestPortal
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    var distance = loopPortal.Position.DistanceFrom(Position);

                    if (distance < shortestDistance)
                    {
                        closestPortal = loopPortal;
                        shortestDistance = distance;
                    }
                }

                return closestPortal;
            }
        }

        public Portal ClosestSpawnPoint
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    if (loopPortal.IsSpawnPoint)
                    {
                        var distance = loopPortal.Position.DistanceFrom(Position);

                        if (distance < shortestDistance)
                        {
                            closestPortal = loopPortal;
                            shortestDistance = distance;
                        }
                    }
                }

                return closestPortal;
            }
        }



        public Character(int id = 0, GameClient client = null)
        {
            Id = id;
            Client = client;
            Items = new CharacterItems(this, 24, 24, 24, 24, 48);
            Skills = new CharacterSkills(this);
            Quests = new CharacterQuests(this);
            SkillMacros = new CharacterSkillMacros(this);
            Buffs = new CharacterBuffs(this);
            Keymap = new CharacterKeymap(this);
            Trocks = new CharacterTrocks(this);
            Memos = new CharacterMemos(this);
            Storage = new CharacterStorage(this);
            Position = new Point(0, 0);
            ControlledMobs = new ControlledMobs(this);
            ControlledNpcs = new ControlledNpcs(this);
            Damage = new CharacterDamage(this);
        }

        public void Initialize()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {
                oPacket.WriteInt(Client.Server.ChannelId);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(true);
                oPacket.WriteShort(0);
                oPacket.WriteInt(Damage.Random.OriginalSeed1);
                oPacket.WriteInt(Damage.Random.OriginalSeed2);
                oPacket.WriteInt(Damage.Random.OriginalSeed3);
                oPacket.WriteLong(-1);
                oPacket.WriteBytes(DataToByteArray());
                oPacket.WriteDateTime(DateTime.UtcNow);

                Client.Send(oPacket);
            }

            IsInitialized = true;

            Map.Characters.Add(this);

            ShowApple();
            UpdateStatsForParty();
            Keymap.Send();
            Memos.Send();
            SkillMacros.Send();

            Task.Factory.StartNew(Client.StartPingCheck);
        }

        private void ShowApple()
        {
            if (Map.MapleId == 1 || Map.MapleId == 2 || Map.MapleId == 809000101 || Map.MapleId == 809000201)
            {
                Client.Send(new PacketWriter(ServerOperationCode.ShowApple));
            }
        }

        public void Update(params StatisticType[] statistics)
        {
            var oPacket = new PacketWriter(ServerOperationCode.StatChanged);
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

            Client.Send(oPacket);
        }

        public void UpdateApperance()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.AvatarModified))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteBool(true);
                oPacket.WriteBytes(AppearanceToByteArray());
                oPacket.WriteByte(0);
                oPacket.WriteShort(0);
                Map.Send(oPacket, this);
            }
        }

        public void Release() => Update();

        public void Notify(string message, NoticeType type = NoticeType.PinkText)
        {
            Client.Send(GamePackets.Notify(message, type));
        }

        public void Revive()
        {
            Health = 50;
            ChangeMap(Map.CachedReference.ReturnMapId);
        }

        public void ChangeMap(int mapId, string portalLabel)
        {
            var portal = DataProvider.Maps.Data[mapId].Portals.FirstOrDefault(x => x.Label == portalLabel);

            if (portal == null)
            {
                LogCheatWarning(CheatType.InvalidMapChange);
                return;
            }

            ChangeMap(mapId, portal.Id);
        }

        public void ChangeMap(int mapId, byte? portalId = null, bool fromPosition = false, Point? position = null)
        {
            Map.Characters.Remove(this);

            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {
                oPacket.WriteInt(Client.Server.ChannelId);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(false);
                oPacket.WriteShort(0);
                oPacket.WriteInt(mapId);
                oPacket.WriteByte(portalId ?? SpawnPoint);
                oPacket.WriteShort(Health);
                oPacket.WriteBool(fromPosition);

                if (fromPosition)
                {
                    oPacket.WritePoint(position);
                }

                oPacket.WriteDateTime(DateTime.Now);

                Client.Send(oPacket);
            }

            Client.Server[mapId].Characters.Add(this);
        }

        public void AddAbility(StatisticType statistic, short mod, bool isReset)
        {
            var maxStat = short.MaxValue;
            var isSubtract = mod < 0;

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

        public void Attack(PacketReader iPacket, AttackType type)
        {
            var attack = new Attack(iPacket, type);

            if (attack.Portals != Portals)
            {
                LogCheatWarning(CheatType.InvalidPortals);
                return;
            }

            Skill skill = null;

            if (attack.SkillId > 0)
            {
                skill = Skills[attack.SkillId];

                skill.Cast();
            }

            // TODO: Modify packet based on attack type.
            using (var oPacket = new PacketWriter(ServerOperationCode.CloseRangeAttack))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteByte((byte)(attack.Targets * 0x10 + attack.Hits));
                oPacket.WriteByte(0); // NOTE: Unknown.
                oPacket.WriteByte((byte)(attack.SkillId != 0 ? skill.CurrentLevel : 0)); // NOTE: Skill level.

                if (attack.SkillId != 0)
                {
                    oPacket.WriteInt(attack.SkillId);
                }

                oPacket.WriteByte(0); // NOTE: Unknown.
                oPacket.WriteByte(attack.Display);
                oPacket.WriteByte(attack.Animation);
                oPacket.WriteByte(attack.WeaponSpeed);
                oPacket.WriteByte(0); // NOTE: Skill mastery.
                oPacket.WriteInt(0); // NOTE: Unknown.

                foreach (var target in attack.Damages)
                {
                    oPacket.WriteInt(target.Key);
                    oPacket.WriteByte(6);

                    foreach (var hit in target.Value)
                    {
                        oPacket.WriteUInt(hit);
                    }
                }

                Map.Send(oPacket, this);
            }

            foreach (var target in attack.Damages)
            {
                if (!Map.Mobs.Contains(target.Key))
                {
                    continue;
                }
                var mob = Map.Mobs[target.Key];
                mob.IsProvoked = true;
                mob.SwitchController(this);
                foreach (var hit in target.Value)
                {
                    if (mob.Damage(this, hit))
                    {
                        mob.Die();
                    }
                }
            }
        }

        public void Talk(string text, bool show = true)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.UserChat))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteBool(IsMaster);
                oPacket.WriteString(text);
                oPacket.WriteBool(show);
                Map.Send(oPacket);
            }
        }

        public void PerformFacialExpression(int expressionId)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Emotion))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteInt(expressionId);
                Map.Send(oPacket, this);
            }
        }

        public void ShowLocalUserEffect(UserEffect effect)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
            {
                oPacket.WriteByte((byte)effect);
                Client.Send(oPacket);
            }
        }

        public void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteByte((int)effect);
                Map.Send(oPacket, skipSelf ? this : null);
            }
        }

        public void Converse(Npc npc, QuestReference quest = null)
        {
            LastQuest = quest;
            npc.Converse(this);
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
                        var skill = Skills[(int)SkillNames.Swordsman.ImprovedMaxHpIncrease];
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

                        var skill = Skills[(int)SkillNames.Magician.ImprovedMaxMpIncrease];
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

        private void UpdateStatsForParty()
        {
            // TODO
        }

        public void LogCheatWarning(CheatType type)
        {
            using (var dbContext = new MapleDbContext())
            {
                dbContext.Cheats.Add(new CheatEntity
                {
                    CharacterId = Id,
                    CheatType = (int)type
                });

                dbContext.SaveChanges();
            }
        }

        internal static void Delete(int characterId)
        {
            using (var dbContext = new MapleDbContext())
            {
                var entity = dbContext.Characters.Find(characterId);
                if (entity != null)
                {
                    dbContext.Characters.Remove(entity);
                }
                dbContext.SaveChanges();
            }
        }

        public void Save()
        {
            if (IsInitialized)
            {
                SpawnPoint = ClosestSpawnPoint?.Id ?? 0;
            }

            using (var dbContext = new MapleDbContext())
            {
                var character = dbContext.Characters
                                       .Where(x => x.Name == Name)
                                       .FirstOrDefault(x => x.WorldId == WorldId);

                if (character == null)
                {
                    _log.LogError($"Cannot find account [{Name}] in World [{WorldId}]");
                    return;
                }

                character.AccountId = AccountId;
                character.AbilityPoints = AbilityPoints;
                character.Dexterity = Dexterity;
                character.Experience = Experience;
                character.Face = Face;
                character.Fame = Fame;
                character.Gender = (byte)_gender;
                character.Hair = Hair;
                character.Health = Health;
                character.Intelligence = _intelligence;
                character.Job = (short)Job;
                character.Level = Level;
                character.Luck = Luck;
                character.MapId = Map?.MapleId ?? ServerConfig.Instance.DefaultMapId;
                character.MaxHealth = MaxHealth;
                character.MaxMana = MaxMana;
                character.Meso = Meso;
                character.Mana = Mana;
                character.Skin = Skin;
                character.SkillPoints = SkillPoints;
                character.SpawnPoint = SpawnPoint;
                character.WorldId = WorldId;
                character.Strength = Strength;
                character.Name = Name;
                character.BuddyListSlots = BuddyListSlots;
                character.GuildRank = GuildRank;
                character.EquipmentSlots = Items.MaxSlots[ItemType.Equipment];
                character.UsableSlots = Items.MaxSlots[ItemType.Usable];
                character.SetupSlots = Items.MaxSlots[ItemType.Setup];
                character.EtceteraSlots = Items.MaxSlots[ItemType.Etcetera];
                character.CashSlots = Items.MaxSlots[ItemType.Cash];

                dbContext.SaveChanges();
            }

            Items.Save();
            Skills.Save();
            Quests.Save();
            Buffs.Save();
            Keymap.Save();
            Trocks.Save();
            SkillMacros.Save();

            _log.LogInformation($"Saved character '{Name}' to database.");
        }

        public void Create()
        {
            using (var dbContext = new MapleDbContext())
            {
                var character = dbContext.Characters
                                       .Where(x => x.Name == Name)
                                       .FirstOrDefault(x => x.WorldId == WorldId);

                if (character != null)
                {
                    _log.LogError($"Error creating acconut - [{Name}] already exists in World [{WorldId}]");
                    return;
                }

                character = new CharacterEntity
                {
                    AccountId = AccountId,
                    AbilityPoints = AbilityPoints,
                    Dexterity = Dexterity,
                    Experience = Experience,
                    Face = Face,
                    Fame = Fame,
                    Gender = (byte)_gender,
                    Hair = Hair,
                    Health = Health,
                    Intelligence = _intelligence,
                    Job = (short)Job,
                    Level = Level,
                    Luck = Luck,
                    MapId = ServerConfig.Instance.DefaultMapId,
                    MaxHealth = MaxHealth,
                    MaxMana = MaxMana,
                    Meso = Meso,
                    Mana = Mana,
                    Skin = Skin,
                    SkillPoints = SkillPoints,
                    SpawnPoint = SpawnPoint,
                    WorldId = WorldId,
                    Strength = Strength,
                    Name = Name,
                    GuildRank = GuildRank,
                    BuddyListSlots = BuddyListSlots,
                    EquipmentSlots = Items.MaxSlots[ItemType.Equipment],
                    UsableSlots = Items.MaxSlots[ItemType.Usable],
                    SetupSlots = Items.MaxSlots[ItemType.Setup],
                    EtceteraSlots = Items.MaxSlots[ItemType.Etcetera],
                    CashSlots = Items.MaxSlots[ItemType.Cash]
                };

                dbContext.Characters.Add(character);
                dbContext.SaveChanges();
                Id = character.Id;

                Items.Save();
                Skills.Save();
                Quests.Save();
                Buffs.Save();
                Keymap.Save();
                Trocks.Save();
            }
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var character = dbContext.Characters.Find(Id);

                if (character == null)
                {
                    _log.LogError($"Cannot find character [{Id}]");
                    return;
                }

                Assigned = true;
                Name = character.Name;
                AccountId = character.AccountId;
                _abilityPoints = character.AbilityPoints;
                _dexterity = character.Dexterity;
                _experience = character.Experience;
                _face = character.Face;
                _fame = character.Fame;
                _gender = (Gender)character.Gender;
                _hair = character.Hair;
                _health = character.Health;
                _intelligence = character.Intelligence;
                _job = (Job)character.Job;
                _level = character.Level;
                _luck = character.Luck;
                Map = Client?.Server[character.MapId] ?? new Map(character.MapId);
                _maxHealth = character.MaxHealth;
                _maxMana = character.MaxMana;
                _meso = character.Meso;
                _mana = character.Mana;
                _skin = character.Skin;
                _strength = character.Strength;
                _skillPoints = character.SkillPoints;
                SpawnPoint = character.SpawnPoint;
                WorldId = character.WorldId;
                _strength = character.Strength;
                GuildRank = character.GuildRank;
                BuddyListSlots = character.BuddyListSlots;
                Items.MaxSlots[ItemType.Equipment] = character.EquipmentSlots;
                Items.MaxSlots[ItemType.Usable] = character.UsableSlots;
                Items.MaxSlots[ItemType.Setup] = character.SetupSlots;
                Items.MaxSlots[ItemType.Etcetera] = character.EtceteraSlots;
                Items.MaxSlots[ItemType.Cash] = character.CashSlots;
                Guild = null;
            }

            Items.Load();
            Skills.Load();
            Quests.Load();
            Buffs.Load();
            Keymap.Load();
            Trocks.Load();
            Memos.Load();
            SkillMacros.Load();
        }
    }
}
