using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class Character : MapObject, IMoveable, ISpawnable, IMapleSavable
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
        private Npc _lastNpc;
        private string _chalkboard;
        private int _itemEffect;

        private readonly ILogger _log = LogManager.Log;

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
                    //throw new StyleUnavailableException();
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
                    //throw new StyleUnavailableException();
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
                if (value > 200)
                {
                    throw new ArgumentException("Level cannot exceed 200.");
                }

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
                            _level++;

                            AbilityPoints += 5;

                            if (Job != Job.Beginner)
                            {
                                SkillPoints += 3;
                            }

                            Update(StatisticType.Level);

                            ShowRemoteUserEffect(UserEffect.LevelUp);
                        }

                        Health = MaxHealth;
                        Mana = MaxMana;
                    }
                    UpdateStatsForParty();
                }
            }
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
                if (value < 0)
                {
                    _mana = 0;
                }
                else if (value > MaxMana)
                {
                    _mana = MaxMana;
                }
                else
                {
                    _mana = value;
                }

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

        public Npc LastNpc
        {
            get => _lastNpc;
            set
            {
                if (value == null)
                {
                    if (value.Scripts.ContainsKey(this))
                    {
                        value.Scripts.Remove(this);
                    }
                }

                _lastNpc = value;
            }
        }

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
                if (IsInitialized)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.ItemEffect))
                    {
                        oPacket.WriteInt(Id);
                        oPacket.WriteInt(_itemEffect);
                        Map.Send(oPacket, this);
                    }
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
            Buffs = new CharacterBuffs(this);
            Keymap = new CharacterKeymap(this);
            Trocks = new CharacterTrocks(this);
            Memos = new CharacterMemos(this);
            Storage = new CharacterStorage(this);
            Position = new Point(0, 0);
            ControlledMobs = new ControlledMobs(this);
            ControlledNpcs = new ControlledNpcs(this);
        }

        public void Initialize()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {

                oPacket.WriteInt(Client.Server.ChannelId);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(true);

                for (var i = 0; i < 3; i++)
                {
                    oPacket.WriteInt(Functions.Random());
                }

                oPacket.WriteShort(-1);

                oPacket.WriteBytes(DataToByteArray());
                oPacket.WriteDateTime(DateTime.UtcNow);

                Client.Send(oPacket);
            }

            IsInitialized = true;

            Map.Characters.Add(this);

            ShowApple();
            UpdateStatsForParty();
            Keymap.Send();
            //Memos.Send();

            //Task.Factory.StartNew(() => { Client.StartPingCheck(); });
        }

        private void ShowApple()
        {
            if (Map.MapleId == 1 || Map.MapleId == 2)
            {
                Client.Send(new PacketWriter(ServerOperationCode.ShowApple));
            }
        }

        public void Update(params StatisticType[] statistics)
        {
            var oPacket = new PacketWriter(ServerOperationCode.StatChanged);
            oPacket.WriteBool(true);
            oPacket.WriteBool(false);

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

        public void Notify(string message, NoticeType type = NoticeType.Pink)
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
                _log.LogWarning($"Character {Id} Attempting to change map to invalid portal: {portalLabel}");
                return;
            }

            ChangeMap(mapId, portal.Id);
        }

        public void ChangeMap(int mapId, byte? portalId = null, bool fromPosition = false, Point position = null)
        {
            Map.Characters.Remove(this);

            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {
                oPacket.WriteInt(Client.Server.ChannelId);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(false);
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
                    case StatisticType.MaxMana:
                        {
                            // TODO: This is way too complicated for now.
                        }
                        break;
                }

                if (!isReset)
                {
                    AbilityPoints -= mod;
                }

                // TODO: Update bonuses.
            }
        }

        public void Attack(PacketReader iPacket, AttackType type)
        {
            var attack = new Attack(iPacket, type);

            if (attack.Portals != Portals)
            {
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

        public void Talk(string text)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.UserChat))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteBool(IsMaster);
                oPacket.WriteString(text);
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
            LastNpc = npc;
            LastQuest = quest;
            LastNpc.Converse(this);
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
                    // TODO: Get addition based on other factors.
                    break;

                case StatisticType.MaxMana:
                    // TODO: Get addition based on other factors.
                    break;
            }

            AbilityPoints -= amount;
        }

        private void UpdateStatsForParty()
        {
            // TODO
        }


        public void MultiTalk(PacketReader iPacket)
        {
            var type = (MultiChatType)iPacket.ReadByte();
            var count = iPacket.ReadByte();

            var recipients = new List<int>();

            while (count-- > 0)
            {
                var recipientId = iPacket.ReadInt();

                recipients.Add(recipientId);
            }

            var text = iPacket.ReadString();

            switch (type)
            {
                case MultiChatType.Buddy:
                    {

                    }
                    break;

                case MultiChatType.Party:
                    {

                    }
                    break;

                case MultiChatType.Guild:
                    {

                    }
                    break;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.GroupMessage))
            {
                oPacket.WriteByte((byte)type);
                oPacket.WriteString(Name);
                oPacket.WriteString(text);

                foreach (var recipient in recipients)
                {
                    Client.Server.GetCharacterById(recipient).Client.Send(oPacket);
                }
            }
        }

        public void UseCommand(PacketReader iPacket)
        {
            var type = (CommandType)iPacket.ReadByte();
            var targetName = iPacket.ReadString();
            var target = Client.Server.GetCharacterByName(targetName);

            switch (type)
            {
                case CommandType.Find:
                    {
                        if (target == null)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket.WriteByte(0x0A);
                                oPacket.WriteString(targetName);
                                oPacket.WriteBool(false);
                                Client.Send(oPacket);
                            }
                        }
                        else
                        {
                            var isInSameChannel = Client.Server.ChannelId == target.Client.Server.ChannelId;

                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket.WriteByte(0x09);
                                oPacket.WriteString(targetName);
                                oPacket.WriteByte((byte)(isInSameChannel ? 1 : 3));
                                oPacket.WriteInt(isInSameChannel ? target.Map.MapleId : target.Client.Server.ChannelId);
                                oPacket.WriteInt(0); // NOTE: Unknown.
                                oPacket.WriteInt(0); // NOTE: Unknown.
                                Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case CommandType.Whisper:
                    {
                        var text = iPacket.ReadString();

                        using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                        {
                            oPacket.WriteByte(10);
                            oPacket.WriteString(targetName);
                            oPacket.WriteBool(target != null);
                            Client.Send(oPacket);
                        }

                        if (target != null)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket.WriteByte(18);
                                oPacket.WriteString(Name);
                                oPacket.WriteByte(Client.Server.ChannelId);
                                oPacket.WriteByte(0);
                                oPacket.WriteString(text);
                                target.Client.Send(oPacket);
                            }
                        }
                    }
                    break;
            }
        }

        public void Interact(PacketReader iPacket)
        {
            var code = (InteractionCode)iPacket.ReadByte();

            switch (code)
            {
                case InteractionCode.Create:
                    {
                        var type = (InteractionType)iPacket.ReadByte();

                        switch (type)
                        {
                            case InteractionType.Omok:
                                {

                                }
                                break;

                            case InteractionType.Trade:
                                {
                                    if (Trade == null)
                                    {
                                        Trade = new Trade(this);
                                    }
                                }
                                break;

                            case InteractionType.PlayerShop:
                                {
                                    var description = iPacket.ReadString();

                                    if (PlayerShop == null)
                                    {
                                        PlayerShop = new PlayerShop(this, description);
                                    }
                                }
                                break;

                            case InteractionType.HiredMerchant:
                                {

                                }
                                break;
                        }
                    }
                    break;

                case InteractionCode.Visit:
                    {
                        if (PlayerShop == null)
                        {
                            var objectId = iPacket.ReadInt();

                            if (Map.PlayerShops.Contains(objectId))
                            {
                                Map.PlayerShops[objectId].AddVisitor(this);
                            }
                        }
                    }
                    break;

                default:
                    {
                        if (Trade != null)
                        {
                            Trade.Handle(this, code, iPacket);
                        }
                        else
                        {
                            PlayerShop?.Handle(this, code, iPacket);
                        }
                    }
                    break;
            }
        }

        public byte[] ToByteArray(bool viewAllCharacters = false)
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteBytes(StatisticsToByteArray());
                oPacket.WriteBytes(AppearanceToByteArray());
                oPacket.WriteBool(IsRanked);

                if (IsRanked)
                {
                    oPacket.WriteInt(Rank);
                    oPacket.WriteInt(RankMove);
                    oPacket.WriteInt(JobRank);
                    oPacket.WriteInt(JobRankMove);
                }

                return oPacket.ToArray();
            }
        }

        public byte[] StatisticsToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(Id);
                oPacket.WriteString(Name, 13);
                oPacket.WriteByte((byte)Gender);
                oPacket.WriteByte(Skin);
                oPacket.WriteInt(Face);
                oPacket.WriteInt(Hair);
                oPacket.WriteLong(0);
                oPacket.WriteLong(0);
                oPacket.WriteLong(0);
                oPacket.WriteByte(Level);
                oPacket.WriteShort((short)Job);
                oPacket.WriteShort(Strength);
                oPacket.WriteShort(Dexterity);
                oPacket.WriteShort(Intelligence);
                oPacket.WriteShort(Luck);
                oPacket.WriteShort(Health);
                oPacket.WriteShort(MaxHealth);
                oPacket.WriteShort(Mana);
                oPacket.WriteShort(MaxMana);
                oPacket.WriteShort(AbilityPoints);
                oPacket.WriteShort(SkillPoints);
                oPacket.WriteInt(Experience);
                oPacket.WriteShort(Fame);
                oPacket.WriteInt(0);
                oPacket.WriteInt(Map.MapleId);
                oPacket.WriteByte(SpawnPoint);
                oPacket.WriteInt(0);

                return oPacket.ToArray();
            }
        }

        public byte[] AppearanceToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                var megaphone = true;

                oPacket.WriteByte((int)Gender);
                oPacket.WriteByte(Skin);
                oPacket.WriteInt(Face);
                oPacket.WriteBool(megaphone);
                oPacket.WriteInt(Hair);

                var visibleLayer = new Dictionary<byte, int>();
                var hiddenLayer = new Dictionary<byte, int>();

                foreach (var item in Items.GetEquipped())
                {
                    var slot = item.AbsoluteSlot;

                    if (slot < 100 && !visibleLayer.ContainsKey(slot))
                    {
                        visibleLayer[slot] = item.MapleId;
                    }
                    else if (slot > 100 && slot != 111)
                    {
                        slot -= 100;

                        if (visibleLayer.ContainsKey(slot))
                        {
                            hiddenLayer[slot] = visibleLayer[slot];
                        }

                        visibleLayer[slot] = item.MapleId;
                    }
                    else if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = item.MapleId;
                    }
                }

                foreach (var entry in visibleLayer)
                {
                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(byte.MaxValue);

                foreach (var entry in hiddenLayer)
                {
                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(byte.MaxValue);

                var cashWeapon = Items[EquipmentSlot.CashWeapon];

                oPacket.WriteInt(cashWeapon?.MapleId ?? 0);

                oPacket.WriteInt(0); // pet id
                oPacket.WriteInt(0); // pet id
                oPacket.WriteInt(0); // pet id

                return oPacket.ToArray();
            }
        }

        public byte[] DataToByteArray()
        {
            var pw = new PacketWriter();
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteByte(BuddyListSlots);
            pw.WriteInt(Meso);
            pw.WriteBytes(Items.ToByteArray());
            pw.WriteBytes(Skills.ToByteArray());
            pw.WriteBytes(Quests.ToByteArray());
            pw.WriteShort(0);// NOTE: Mini games record.
            pw.WriteShort(0);// NOTE: Rings (1).
            pw.WriteShort(0);// NOTE: Rings (2). 
            pw.WriteShort(0);// NOTE: Rings (3).
            pw.WriteBytes(Trocks.RegularToByteArray());
            pw.WriteBytes(Trocks.VipToByteArray());
            return pw.ToArray();
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.UserEnterField);

            oPacket.WriteInt(Id);
            oPacket.WriteByte(Level);
            oPacket.WriteString(Name);
            oPacket.WriteString(Guild?.Name);
            oPacket.WriteShort(Guild?.LogoBg ?? 0);
            oPacket.WriteByte(Guild?.LogoBgColor ?? 0);
            oPacket.WriteShort(Guild?.Logo ?? 0);
            oPacket.WriteByte(Guild?.LogoColor ?? 0);
            oPacket.WriteBytes(Buffs.ToByteArray());
            oPacket.WriteShort((short)Job);
            oPacket.WriteBytes(AppearanceToByteArray());
            oPacket.WriteInt(Items.Available(5110000));
            oPacket.WriteInt(ItemEffect);
            oPacket.WriteInt(Item.GetType(Chair) == ItemType.Setup ? Chair : 0);
            oPacket.WritePoint(Position);
            oPacket.WriteByte(Stance);
            oPacket.WriteShort(Foothold);
            oPacket.WriteByte(0);
            oPacket.WriteByte(0);
            oPacket.WriteInt(1);
            oPacket.WriteLong(0);

            if (PlayerShop != null && PlayerShop.Owner == this)
            {

                oPacket.WriteByte((byte)InteractionType.PlayerShop);
                oPacket.WriteInt(PlayerShop.ObjectId);
                oPacket.WriteString(PlayerShop.Description);
                oPacket.WriteBool(PlayerShop.IsPrivate);
                oPacket.WriteByte(0);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)(PlayerShop.IsFull ? 1 : 2)); // NOTE: Visitor availability.
                oPacket.WriteByte(0);
            }
            else
            {
                oPacket.WriteByte(0);
            }

            var hasChalkboard = !string.IsNullOrEmpty(Chalkboard);

            oPacket.WriteBool(hasChalkboard);

            if (hasChalkboard)
            {
                oPacket.WriteString(Chalkboard);
            }

            oPacket.WriteByte(0); // NOTE: Couple ring.
            oPacket.WriteByte(0); // NOTE: Friendship ring.
            oPacket.WriteByte(0); // NOTE: Marriage ring.
            oPacket.WriteByte(0);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.UserLeaveField);
            oPacket.WriteInt(Id);
            return oPacket;
        }

        internal static bool CharacterExists(string name)
        {
            using (var dbContext = new MapleDbContext())
            {
                return dbContext.Characters.Any(x => x.Name == name);
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
        }
    }
}