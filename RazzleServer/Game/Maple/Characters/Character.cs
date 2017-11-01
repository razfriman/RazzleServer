using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Commands;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class Character : MapObject, IMoveable, ISpawnable
    {
        public GameClient Client { get; private set; }



        public int ID { get; set; }
        public int AccountID { get; set; }
        public byte WorldID { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; private set; }

        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int? GuildRank { get; set; }

        public CharacterItems Items { get; private set; }
        public CharacterSkills Skills { get; private set; }
        public CharacterQuests Quests { get; private set; }
        public CharacterBuffs Buffs { get; private set; }
        public CharacterKeymap Keymap { get; private set; }
        public CharacterTrocks Trocks { get; private set; }
        public CharacterMemos Memos { get; private set; }
        public CharacterStorage Storage { get; private set; }
        public CharacterVariables Variables { get; private set; }
        public ControlledMobs ControlledMobs { get; private set; }
        public ControlledNpcs ControlledNpcs { get; private set; }
        public Trade Trade { get; set; }
        public PlayerShop PlayerShop { get; set; }

        private DateTime LastHealthHealOverTime = new DateTime();
        private DateTime LastManaHealOverTime = new DateTime();

        private Gender gender;
        private byte skin;
        private int face;
        private int hair;
        private byte level;
        private Job job;
        private short strength;
        private short dexterity;
        private short intelligence;
        private short luck;
        private short health;
        private short maxHealth;
        private short mana;
        private short maxMana;
        private short abilityPoints;
        private short skillPoints;
        private int experience;
        private short fame;
        private int meso;
        private Npc lastNpc;
        private Quest lastQuest;
        private string chalkboard;

        private static readonly ILogger Log = LogManager.Log;

        public Gender Gender
        {
            get
            {
                return gender;
            }
            set
            {
                gender = value;

                if (IsInitialized)
                {
                    var oPacket = new PacketWriter(ServerOperationCode.SetGender);
                    oPacket.WriteByte((byte)Gender);
                    Client.Send(oPacket);
                }
            }
        }

        public byte Skin
        {
            get
            {
                return skin;
            }
            set
            {
                if (!DataProvider.Styles.Skins.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                skin = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Skin);
                    UpdateApperance();
                }
            }
        }

        public int Face
        {
            get
            {
                return face;
            }
            set
            {
                if ((Gender == Gender.Male && !DataProvider.Styles.MaleFaces.Contains(value)) ||
                    Gender == Gender.Female && !DataProvider.Styles.FemaleFaces.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                face = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Face);
                    UpdateApperance();
                }
            }
        }

        public int Hair
        {
            get
            {
                return hair;
            }
            set
            {
                if ((Gender == Gender.Male && !DataProvider.Styles.MaleHairs.Contains(value)) ||
                    Gender == Gender.Female && !DataProvider.Styles.FemaleHairs.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                hair = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Hair);
                    UpdateApperance();
                }
            }
        }

        public int HairStyleOffset
        {
            get
            {
                return (Hair / 10) * 10;
            }
        }

        public int FaceStyleOffset
        {
            get
            {
                return (Face - (10 * (Face / 10))) + (Gender == Gender.Male ? 20000 : 21000);
            }
        }

        public int HairColorOffset
        {
            get
            {
                return Hair - (10 * (Hair / 10));
            }
        }

        public int FaceColorOffset
        {
            get
            {
                return ((Face / 100) - (10 * (Face / 1000))) * 100;
            }
        }

        // TODO: Update party's properties.
        public byte Level
        {
            get
            {
                return level;
            }
            set
            {
                if (value > 200)
                {
                    throw new ArgumentException("Level cannot exceed 200.");
                }

                int delta = value - Level;

                if (!IsInitialized)
                {
                    level = value;
                }
                else
                {
                    if (delta < 0)
                    {
                        level = value;

                        Update(StatisticType.Level);
                    }
                    else
                    {
                        for (int i = 0; i < delta; i++)
                        {
                            // TODO: Health/Mana improvement.

                            level++;

                            if (IsCygnus)
                            {
                                AbilityPoints += 6;
                            }
                            else
                            {
                                AbilityPoints += 5;
                            }

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
                }
            }
        }

        // TODO: Update party's properties.
        public Job Job
        {
            get
            {
                return job;
            }
            set
            {
                job = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Job);

                    ShowRemoteUserEffect(UserEffect.JobChanged);
                }
            }
        }

        public short Strength
        {
            get
            {
                return strength;
            }
            set
            {
                strength = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Strength);
                }
            }
        }

        public short Dexterity
        {
            get
            {
                return dexterity;
            }
            set
            {
                dexterity = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Dexterity);
                }
            }
        }

        public short Intelligence
        {
            get
            {
                return intelligence;
            }
            set
            {
                intelligence = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Intelligence);
                }
            }
        }

        public short Luck
        {
            get
            {
                return luck;
            }
            set
            {
                luck = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Luck);
                }
            }
        }

        public short Health
        {
            get
            {
                return health;
            }
            set
            {
                if (value < 0)
                {
                    health = 0;
                }
                else if (value > MaxHealth)
                {
                    health = MaxHealth;
                }
                else
                {
                    health = value;
                }

                if (IsInitialized)
                {
                    Update(StatisticType.Health);
                }
            }
        }

        public short MaxHealth
        {
            get
            {
                return maxHealth;
            }
            set
            {
                maxHealth = value;

                if (IsInitialized)
                {
                    Update(StatisticType.MaxHealth);
                }
            }
        }

        public short Mana
        {
            get
            {
                return mana;
            }
            set
            {
                if (value < 0)
                {
                    mana = 0;
                }
                else if (value > MaxMana)
                {
                    mana = MaxMana;
                }
                else
                {
                    mana = value;
                }

                if (IsInitialized)
                {
                    Update(StatisticType.Mana);
                }
            }
        }

        public short MaxMana
        {
            get
            {
                return maxMana;
            }
            set
            {
                maxMana = value;

                if (IsInitialized)
                {
                    Update(StatisticType.MaxMana);
                }
            }
        }

        public short AbilityPoints
        {
            get
            {
                return abilityPoints;
            }
            set
            {
                abilityPoints = value;

                if (IsInitialized)
                {
                    Update(StatisticType.AbilityPoints);
                }
            }
        }

        public short SkillPoints
        {
            get
            {
                return skillPoints;
            }
            set
            {
                skillPoints = value;

                if (IsInitialized)
                {
                    Update(StatisticType.SkillPoints);
                }
            }
        }

        public int Experience
        {
            get
            {
                return experience;
            }
            set
            {
                int delta = value - experience;

                experience = value;

                if (Client.Server.World.AllowMultiLeveling)
                {
                    while (experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        experience -= ExperienceTables.CharacterLevel[Level];

                        Level++;
                    }
                }
                else
                {
                    if (experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        experience -= ExperienceTables.CharacterLevel[Level];

                        Level++;
                    }

                    if (experience >= ExperienceTables.CharacterLevel[Level])
                    {
                        experience = ExperienceTables.CharacterLevel[Level] - 1;
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
            get => fame;
            set
            {
                fame = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Fame);
                }
            }
        }

        public int Meso
        {
            get => meso;
            set
            {
                meso = value;

                if (IsInitialized)
                {
                    Update(StatisticType.Mesos);
                }
            }
        }

        public bool IsAlive => Health > 0;

        public bool IsMaster => Client.Account?.IsMaster ?? false;

        // TODO: Improve this check.
        public bool IsCygnus => (short)Job >= 1000 && (short)Job <= 2000;

        public bool FacesLeft
        {
            get
            {
                return Stance % 2 == 0;
            }
        }

        public bool IsRanked
        {
            get
            {
                return Level >= 30;
            }
        }

        public Npc LastNpc
        {
            get
            {
                return lastNpc;
            }
            set
            {
                if (value == null)
                {
                    if (value.Scripts.ContainsKey(this))
                    {
                        value.Scripts.Remove(this);
                    }
                }

                lastNpc = value;
            }
        }

        public Quest LastQuest
        {
            get
            {
                return lastQuest;
            }
            set
            {
                lastQuest = value;

                // TODO: Add checks.
            }
        }

        public string Chalkboard
        {
            get
            {
                return chalkboard;
            }
            set
            {
                chalkboard = value;

                using (var oPacket = new PacketWriter(ServerOperationCode.Chalkboard))
                {
                    oPacket.WriteInt(ID);
                    oPacket.WriteBool(!string.IsNullOrEmpty(Chalkboard));
                    oPacket.WriteString(Chalkboard);

                    Map.Broadcast(oPacket);
                }
            }
        }

        public Portal ClosestPortal
        {
            get
            {
                Portal closestPortal = null;
                double shortestDistance = double.PositiveInfinity;

                foreach (Portal loopPortal in Map.Portals)
                {
                    double distance = loopPortal.Position.DistanceFrom(Position);

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
                double shortestDistance = double.PositiveInfinity;

                foreach (Portal loopPortal in Map.Portals)
                {
                    if (loopPortal.IsSpawnPoint)
                    {
                        double distance = loopPortal.Position.DistanceFrom(Position);

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

        private bool Assigned { get; set; }

        public Character(int id = 0, GameClient client = null)
        {
            ID = id;
            Client = client;

            Items = new CharacterItems(this, 24, 24, 24, 24, 48);
            Skills = new CharacterSkills(this);
            Quests = new CharacterQuests(this);
            Buffs = new CharacterBuffs(this);
            Keymap = new CharacterKeymap(this);
            Trocks = new CharacterTrocks(this);
            Memos = new CharacterMemos(this);
            Storage = new CharacterStorage(this);
            Variables = new CharacterVariables(this);

            Position = new Point(0, 0);
            ControlledMobs = new ControlledMobs(this);
            ControlledNpcs = new ControlledNpcs(this);
        }

        public void Load()
        {
            Datum datum = new Datum("characters");

            datum.Populate("ID = {0}", ID);

            ID = (int)datum["ID"];
            Assigned = true;

            AccountID = (int)datum["AccountID"];
            WorldID = (byte)datum["WorldID"];
            Name = (string)datum["Name"];
            Gender = (Gender)datum["Gender"];
            Skin = (byte)datum["Skin"];
            Face = (int)datum["Face"];
            Hair = (int)datum["Hair"];
            Level = (byte)datum["Level"];
            Job = (Job)datum["Job"];
            Strength = (short)datum["Strength"];
            Dexterity = (short)datum["Dexterity"];
            Intelligence = (short)datum["Intelligence"];
            Luck = (short)datum["Luck"];
            MaxHealth = (short)datum["MaxHealth"];
            MaxMana = (short)datum["MaxMana"];
            Health = (short)datum["Health"];
            Mana = (short)datum["Mana"];
            AbilityPoints = (short)datum["AbilityPoints"];
            SkillPoints = (short)datum["SkillPoints"];
            Experience = (int)datum["Experience"];
            Fame = (short)datum["Fame"];
            Map = DataProvider.Maps[(int)datum["MapID"]];
            SpawnPoint = (byte)datum["SpawnPoint"];
            Meso = (int)datum["Meso"];

            Items.MaxSlots[ItemType.Equipment] = (byte)datum["EquipmentSlots"];
            Items.MaxSlots[ItemType.Usable] = (byte)datum["UsableSlots"];
            Items.MaxSlots[ItemType.Setup] = (byte)datum["SetupSlots"];
            Items.MaxSlots[ItemType.Etcetera] = (byte)datum["EtceteraSlots"];
            Items.MaxSlots[ItemType.Cash] = (byte)datum["CashSlots"];

            Items.Load();
            Skills.Load();
            Quests.Load();
            Buffs.Load();
            Keymap.Load();
            Trocks.Load();
            Memos.Load();
            Variables.Load();
        }

        public void Save()
        {
            if (IsInitialized)
            {
                SpawnPoint = ClosestSpawnPoint.ID;
            }

            Datum datum = new Datum("characters");

            datum["AccountID"] = AccountID;
            datum["WorldID"] = WorldID;
            datum["Name"] = Name;
            datum["Gender"] = (byte)Gender;
            datum["Skin"] = Skin;
            datum["Face"] = Face;
            datum["Hair"] = Hair;
            datum["Level"] = Level;
            datum["Job"] = (short)Job;
            datum["Strength"] = Strength;
            datum["Dexterity"] = Dexterity;
            datum["Intelligence"] = Intelligence;
            datum["Luck"] = Luck;
            datum["Health"] = Health;
            datum["MaxHealth"] = MaxHealth;
            datum["Mana"] = Mana;
            datum["MaxMana"] = MaxMana;
            datum["AbilityPoints"] = AbilityPoints;
            datum["SkillPoints"] = SkillPoints;
            datum["Experience"] = Experience;
            datum["Fame"] = Fame;
            datum["MapID"] = Map.MapleID;
            datum["SpawnPoint"] = SpawnPoint;
            datum["Meso"] = Meso;

            datum["EquipmentSlots"] = Items.MaxSlots[ItemType.Equipment];
            datum["UsableSlots"] = Items.MaxSlots[ItemType.Usable];
            datum["SetupSlots"] = Items.MaxSlots[ItemType.Setup];
            datum["EtceteraSlots"] = Items.MaxSlots[ItemType.Etcetera];
            datum["CashSlots"] = Items.MaxSlots[ItemType.Cash];

            if (Assigned)
            {
                datum.Update("ID = {0}", ID);
            }
            else
            {
                ID = datum.InsertAndReturnID();
                Assigned = true;
            }

            Items.Save();
            Skills.Save();
            Quests.Save();
            Buffs.Save();
            Keymap.Save();
            Trocks.Save();
            Variables.Save();

            Log.LogInformation($"Saved character '{Name}' to database.");
        }

        public void Initialize()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {

                oPacket.WriteInt(Client.Server.ChannelID);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(true);
                oPacket.WriteShort(0); // NOTE: Floating messages at top corner.

                for (int i = 0; i < 3; i++)
                {
                    oPacket.WriteInt(Functions.Random());
                }

                oPacket.WriteBytes(DataToByteArray());
                oPacket.WriteDateTime(DateTime.UtcNow);

                Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.ClaimSvrStatusChanged))
            {
                oPacket.WriteBool(true);

                Client.Send(oPacket);
            }

            IsInitialized = true;

            Map.Characters.Add(this);

            Keymap.Send();

            Memos.Send();
        }

        public void Update(params StatisticType[] statistics)
        {
            var oPacket = new PacketWriter(ServerOperationCode.StatChanged);
            oPacket.WriteBool(true); // TODO: bOnExclRequest.

            int flag = 0;

            foreach (StatisticType statistic in statistics)
            {
                flag |= (int)statistic;
            }

            oPacket.WriteInt(flag);

            Array.Sort(statistics);

            foreach (StatisticType statistic in statistics)
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
                oPacket.WriteInt(ID);
                oPacket.WriteBool(true);
                oPacket.WriteBytes(AppearanceToByteArray());
                oPacket.WriteByte(0);
                oPacket.WriteShort(0);

                Map.Broadcast(oPacket, this);
            }
        }

        public void Release()
        {
            Update();
        }

        public void Notify(string message, NoticeType type = NoticeType.Pink)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
            {
                oPacket.WriteByte((byte)type);

                if (type == NoticeType.Ticker)
                {
                    oPacket.WriteBool(!string.IsNullOrEmpty(message));
                }

                oPacket.WriteString(message);

                Client.Send(oPacket);
            }
        }

        public void ChangeMap(PacketReader iPacket)
        {
           
        }

        public void Revive()
        {
            Health = 50;
            ChangeMap(Map.ReturnMapID);
        }

        public void ChangeMap(int mapID, string portalLabel)
        {
            this.ChangeMap(mapID, DataProvider.Maps[mapID].Portals[portalLabel].ID);
        }

        public void ChangeMap(int mapID, byte portalID = 0, bool fromPosition = false, Point position = null)
        {
            Map.Characters.Remove(this);

            using (var oPacket = new PacketWriter(ServerOperationCode.SetField))
            {
                oPacket.WriteInt(Client.Server.ChannelID);
                oPacket.WriteByte(++Portals);
                oPacket.WriteBool(false);
                oPacket.WriteShort(0);
                oPacket.WriteByte(0);
                oPacket.WriteInt(mapID);
                oPacket.WriteByte(SpawnPoint);
                oPacket.WriteShort(Health);
                oPacket.WriteBool(fromPosition);

                if (fromPosition)
                {
                    oPacket.WritePoint(position);
                }

                oPacket.WriteDateTime(DateTime.Now);

                Client.Send(oPacket);
            }

            DataProvider.Maps[mapID].Characters.Add(this);
        }

        public void AddAbility(StatisticType statistic, short mod, bool isReset)
        {
            short maxStat = short.MaxValue; // TODO: Should this be a setting?
            bool isSubtract = mod < 0;

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

        public void Move(PacketReader iPacket)
        {
            byte portals = iPacket.ReadByte();

            if (portals != Portals)
            {
                return;
            }

            iPacket.ReadInt(); // NOE: Unknown.

            Movements movements = Movements.Decode(iPacket);

            Position = movements.Position;
            Foothold = movements.Foothold;
            Stance = movements.Stance;

            using (var oPacket = new PacketWriter(ServerOperationCode.Move))
            {

                oPacket.WriteInt(ID);
                oPacket.WriteBytes(movements.ToByteArray());

                Map.Broadcast(oPacket, this);
            }

            if (Foothold == 0)
            {
                // NOTE: Player is floating in the air.
                // GMs might be legitmately in this state due to GM fly.
                // We shouldn't mess with them because they have the tools toget out of falling off the map anyway.

                // TODO: Attempt to find foothold.
                // If none found, check the player fall counter.
                // If it's over 3, reset the player's map.
            }
        }

        public void Attack(PacketReader iPacket, AttackType type)
        {
            Attack attack = new Attack(iPacket, type);

            if (attack.Portals != Portals)
            {
                return;
            }

            Skill skill = null;

            if (attack.SkillID > 0)
            {
                skill = Skills[attack.SkillID];

                skill.Cast();
            }

            // TODO: Modify packet based on attack type.
            using (var oPacket = new PacketWriter(ServerOperationCode.CloseRangeAttack))
            {
                oPacket.WriteInt(ID);
                oPacket.WriteByte((byte)((attack.Targets * 0x10) + attack.Hits));
                oPacket.WriteByte(0); // NOTE: Unknown.
                oPacket.WriteByte((byte)(attack.SkillID != 0 ? skill.CurrentLevel : 0)); // NOTE: Skill level.

                if (attack.SkillID != 0)
                {
                    oPacket.WriteInt(attack.SkillID);
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

                    foreach (uint hit in target.Value)
                    {
                        oPacket.WriteUInt(hit);
                    }
                }

                Map.Broadcast(oPacket, this);
            }

            foreach (KeyValuePair<int, List<uint>> target in attack.Damages)
            {
                Mob mob;

                try
                {
                    mob = Map.Mobs[target.Key];
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }

                mob.IsProvoked = true;
                mob.SwitchController(this);

                foreach (uint hit in target.Value)
                {
                    if (mob.Damage(this, hit))
                    {
                        mob.Die();
                    }
                }
            }
        }

        private const sbyte BumpDamage = -1;
        private const sbyte MapDamage = -2;

        public void Damage(PacketReader iPacket)
        {
            iPacket.Skip(4); // NOTE: Ticks.
            sbyte type = (sbyte)iPacket.ReadByte();
            iPacket.ReadByte(); // NOTE: Elemental type.
            int damage = iPacket.ReadInt();
            bool damageApplied = false;
            bool deadlyAttack = false;
            byte hit = 0;
            byte stance = 0;
            int disease = 0;
            byte level = 0;
            short mpBurn = 0;
            int mobObjectID = 0;
            int mobID = 0;
            int noDamageSkillID = 0;

            if (type != MapDamage)
            {
                mobID = iPacket.ReadInt();
                mobObjectID = iPacket.ReadInt();

                Mob mob;

                try
                {
                    mob = Map.Mobs[mobObjectID];
                }
                catch (KeyNotFoundException)
                {
                    return;
                }

                if (mobID != mob.MapleID)
                {
                    return;
                }

                if (type != BumpDamage)
                {
                    // TODO: Get mob attack and apply to disease/level/mpBurn/deadlyAttack.
                }
            }

            hit = iPacket.ReadByte();
            byte reduction = iPacket.ReadByte();
            iPacket.ReadByte(); // NOTE: Unknown.

            if (reduction != 0)
            {
                // TODO: Return damage (Power Guard).
            }

            if (type == MapDamage)
            {
                level = iPacket.ReadByte();
                disease = iPacket.ReadInt();
            }
            else
            {
                stance = iPacket.ReadByte();

                if (stance > 0)
                {
                    // TODO: Power Stance.
                }
            }

            if (damage == -1)
            {
                // TODO: Validate no damage skills.
            }

            if (disease > 0 && damage != 0)
            {
                // NOTE: Fake/Guardian don't prevent disease.
                // TODO: Add disease buff.
            }

            if (damage > 0)
            {
                // TODO: Check for Meso Guard.
                // TODO: Check for Magic Guard.
                // TODO: Check for Achilles.

                if (!damageApplied)
                {
                    if (deadlyAttack)
                    {
                        // TODO: Deadly attack function.
                    }
                    else
                    {
                        Health -= (short)damage;
                    }

                    if (mpBurn > 0)
                    {
                        Mana -= mpBurn;
                    }
                }

                // TODO: Apply damage to buffs.
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Hit))
            {
                oPacket.WriteInt(ID);
                oPacket.WriteByte(type);

                switch (type)
                {
                    case MapDamage:
                        {
                            oPacket.WriteInt(damage);
                            oPacket.WriteInt(damage);
                        }
                        break;

                    default:
                        {
                            oPacket.WriteInt(damage); // TODO: ... or PGMR damage.
                            oPacket.WriteInt(mobID);
                            oPacket.WriteByte(hit);
                            oPacket.WriteByte(reduction);

                            if (reduction > 0)
                            {
                                // TODO: PGMR stuff.
                            }

                            oPacket.WriteByte(stance);
                            oPacket.WriteInt(damage);

                            if (noDamageSkillID > 0)
                            {
                                oPacket.WriteInt(noDamageSkillID);
                            }
                        }
                        break;
                }

                Map.Broadcast(oPacket, this);
            }
        }

        public void Talk(PacketReader iPacket)
        {
            string text = iPacket.ReadString();
            bool shout = iPacket.ReadBool(); // NOTE: Used for skill macros.

            if (text.StartsWith(ServerConfig.Instance.CommandIndicator.ToString()))
            {
                CommandFactory.Execute(this, text);
            }
            else
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.UserChat))
                {
                    oPacket.WriteInt(ID);
                    oPacket.WriteBool(IsMaster);
                    oPacket.WriteString(text);
                    oPacket.WriteBool(shout);

                    Map.Broadcast(oPacket);
                }
            }
        }

        public void Express(PacketReader iPacket)
        {
            int expressionID = iPacket.ReadInt();

            if (expressionID > 7) // NOTE: Cash facial expression.
            {
                int mapleID = 5159992 + expressionID;

                // TODO: Validate if item exists.
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Emotion))
            {
                oPacket.WriteInt(ID);
                oPacket.WriteInt(expressionID);

                Map.Broadcast(oPacket, this);
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
                oPacket.WriteInt(ID);
                oPacket.WriteByte((int)effect);

                Map.Broadcast(oPacket, skipSelf ? this : null);
            }
        }

        public void Converse(int mapleID)
        {
            // TODO.
        }

        public void Converse(PacketReader iPacket)
        {
            int objectID = iPacket.ReadInt();

            this.Converse(Map.Npcs[objectID]);
        }

        public void Converse(Npc npc, Quest quest = null)
        {
            LastNpc = npc;
            LastQuest = quest;

            LastNpc.Converse(this);
        }

        public void DistributeAP(StatisticType type, short amount = 1)
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
        }

        public void DistributeAP(PacketReader iPacket)
        {
            if (AbilityPoints == 0)
            {
                return;
            }

            iPacket.ReadInt(); // NOTE: Ticks.
            StatisticType type = (StatisticType)iPacket.ReadInt();

            DistributeAP(type);
            AbilityPoints--;
        }

        public void AutoDistributeAP(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            int count = iPacket.ReadInt(); // NOTE: There are always 2 primary stats for each job, but still.

            int total = 0;

            for (int i = 0; i < count; i++)
            {
                StatisticType type = (StatisticType)iPacket.ReadInt();
                int amount = iPacket.ReadInt();

                if (amount > AbilityPoints || amount < 0)
                {
                    return;
                }

                DistributeAP(type, (short)amount);

                total += amount;
            }

            AbilityPoints -= (short)total;
        }

        public void HealOverTime(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            iPacket.ReadInt(); // NOTE: Unknown.
            short healthAmount = iPacket.ReadShort(); // TODO: Validate
            short manaAmount = iPacket.ReadShort(); // TODO: Validate

            if (healthAmount != 0)
            {
                if ((DateTime.Now - LastHealthHealOverTime).TotalSeconds < 2)
                {
                    return;
                }
                else
                {
                    Health += healthAmount;
                    LastHealthHealOverTime = DateTime.Now;
                }
            }

            if (manaAmount != 0)
            {
                if ((DateTime.Now - LastManaHealOverTime).TotalSeconds < 2)
                {
                    return;
                }
                else
                {
                    Mana += manaAmount;
                    LastManaHealOverTime = DateTime.Now;
                }
            }
        }

        public void DistributeSP(PacketReader iPacket)
        {
            if (SkillPoints == 0)
            {
                return;
            }

            iPacket.ReadInt(); // NOTE: Ticks.
            int mapleID = iPacket.ReadInt();

            if (!Skills.Contains(mapleID))
            {
                Skills.Add(new Skill(mapleID));
            }

            Skill skill = Skills[mapleID];

            // TODO: Check for skill requirements.

            if (skill.IsFromBeginner)
            {
                // TODO: Handle beginner skills.
            }

            if (skill.CurrentLevel + 1 <= skill.MaxLevel)
            {
                if (!skill.IsFromBeginner)
                {
                    SkillPoints--;
                }

                Release();

                skill.CurrentLevel++;
            }
        }

        public void DropMeso(PacketReader iPacket)
        {
            iPacket.Skip(4); // NOTE: tRequestTime (ticks).
            int amount = iPacket.ReadInt();

            if (amount > Meso || amount < 10 || amount > 50000)
            {
                return;
            }

            Meso -= amount;

            Meso meso = new Meso(amount)
            {
                Dropper = this,
                Owner = null
            };

            Map.Drops.Add(meso);
        }

        public void InformOnCharacter(PacketReader iPacket)
        {
            iPacket.Skip(4);
            int characterID = iPacket.ReadInt();

            Character target;

            try
            {
                target = Map.Characters[characterID];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            if (target.IsMaster && !IsMaster)
            {
                return;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.CharacterInformation))
            {
                oPacket.WriteInt(target.ID);
                oPacket.WriteByte(target.Level);
                oPacket.WriteShort((int)target.Job);
                oPacket.WriteShort(target.Fame);
                oPacket.WriteBool(false); // NOTE: Marriage.
                oPacket.WriteString("-"); // NOTE: Guild name.
                oPacket.WriteString("-"); // NOTE: Alliance name.
                oPacket.WriteByte(0); // NOTE: Unknown.
                oPacket.WriteByte(0); // NOTE: Pets.
                oPacket.WriteByte(0); // NOTE: Mount.
                oPacket.WriteByte(0); // NOTE: Wishlist.
                oPacket.WriteInt(0); // NOTE: Monster Book level.
                oPacket.WriteInt(0); // NOTE: Monster Book normal cards. 
                oPacket.WriteInt(0); // NOTE: Monster Book special cards.
                oPacket.WriteInt(0); // NOTE: Monster Book total cards.
                oPacket.WriteInt(0); // NOTE: Monster Book cover.
                oPacket.WriteInt(0); // NOTE: Medal ID.
                oPacket.WriteShort(0); // NOTE: Medal quests.

                Client.Send(oPacket);
            }
        }

        // TODO: Should we refactor it in a way that sends it to the buddy/party/guild objects
        // instead of pooling the world for characters?
        public void MultiTalk(PacketReader iPacket)
        {
            MultiChatType type = (MultiChatType)iPacket.ReadByte();
            byte count = iPacket.ReadByte();

            List<int> recipients = new List<int>();

            while (count-- > 0)
            {
                int recipientID = iPacket.ReadInt();

                recipients.Add(recipientID);
            }

            string text = iPacket.ReadString();

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

                case MultiChatType.Alliance:
                    {

                    }
                    break;
            }

            // NOTE: This is here for convinience. If you accidently use another text window (like party) and not the main text window,
            // your commands won't be shown but instead executed from there as well.
            if (text.StartsWith(ServerConfig.Instance.CommandIndicator))
            {
                CommandFactory.Execute(this, text);
            }
            else
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.GroupMessage))
                {
                    oPacket.WriteByte((byte)type);
                    oPacket.WriteString(Name);
                    oPacket.WriteString(text);

                    foreach (int recipient in recipients)
                    {
                        //this.Client.World.GetCharacter(recipient).Client.Send(oPacket);
                    }
                }
            }
        }

        // TODO: Cash Shop/MTS scenarios.
        public void UseCommand(PacketReader iPacket)
        {
            /*CommandType type = (CommandType)iPacket.ReadByte();
            string targetName = iPacket.ReadString();

            Character target = null;// this.Client.World.GetCharacter(targetName);

            switch (type)
            {
                case CommandType.Find:
                    {
                        if (target == null)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket
                                    oPacket.WriteByte(0x0A)
                                    oPacket.WriteString(targetName)
                                    oPacket.WriteBool(false);

                                this.Client.Send(oPacket);
                            }
                        }
                        else
                        {
                            bool isInSameChannel = this.Client.ChannelID == target.Client.ChannelID;

                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket
                                    oPacket.WriteByte(0x09)
                                    oPacket.WriteString(targetName)
                                    oPacket.WriteByte((byte)(isInSameChannel ? 1 : 3))
                                    oPacket.WriteInt(isInSameChannel ? target.Map.MapleID : target.Client.ChannelID)
                                    oPacket.WriteInt() // NOTE: Unknown.
                                    oPacket.WriteInt(); // NOTE: Unknown.

                                this.Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case CommandType.Whisper:
                    {
                        string text = iPacket.ReadString();

                        using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                        {
                            oPacket
                                oPacket.WriteByte(10)
                                oPacket.WriteString(targetName)
                                oPacket.WriteBool(target != null);

                            this.Client.Send(oPacket);
                        }

                        if (target != null)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                            {
                                oPacket
                                    oPacket.WriteByte(18)
                                    oPacket.WriteString(this.Name)
                                    oPacket.WriteByte(this.Client.ChannelID)
                                    oPacket.WriteByte() // NOTE: Unknown.
                                    oPacket.WriteString(text);

                                target.Client.Send(oPacket);
                            }
                        }
                    }
                    break;
            }*/
        }

        public void Interact(PacketReader iPacket)
        {
            InteractionCode code = (InteractionCode)iPacket.ReadByte();

            switch (code)
            {
                case InteractionCode.Create:
                    {
                        InteractionType type = (InteractionType)iPacket.ReadByte();

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
                                    string description = iPacket.ReadString();

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
                            int objectID = iPacket.ReadInt();

                            if (Map.PlayerShops.Contains(objectID))
                            {
                                Map.PlayerShops[objectID].AddVisitor(this);
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
                        else if (PlayerShop != null)
                        {
                            PlayerShop.Handle(this, code, iPacket);
                        }
                    }
                    break;
            }
        }

        public void UseAdminCommand(PacketReader iPacket)
        {
            if (!IsMaster)
            {
                return;
            }

            AdminCommandType type = (AdminCommandType)iPacket.ReadByte();

            switch (type)
            {
                case AdminCommandType.Hide:
                    {
                        bool hide = iPacket.ReadBool();

                        if (hide)
                        {
                            // TODO: Add SuperGM's hide buff.
                        }
                        else
                        {
                            // TOOD: Remove SuperGM's hide buff.
                        }
                    }
                    break;

                case AdminCommandType.Send:
                    {
                        string name = iPacket.ReadString();
                        int destinationID = iPacket.ReadInt();

                        Character target = null;// this.Client.World.GetCharacter(name);

                        if (target != null)
                        {
                            target.ChangeMap(destinationID);
                        }
                        else
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                            {

                                oPacket.WriteByte(6);
                                oPacket.WriteByte(1);

                                Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case AdminCommandType.Summon:
                    {
                        int mobID = iPacket.ReadInt();
                        int count = iPacket.ReadInt();

                        if (DataProvider.Mobs.Contains(mobID))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                Map.Mobs.Add(new Mob(mobID, Position));
                            }
                        }
                        else
                        {
                            Notify("invalid mob: " + mobID); // TODO: Actual message.
                        }
                    }
                    break;

                case AdminCommandType.CreateItem:
                    {
                        int itemID = iPacket.ReadInt();

                        Items.Add(new Item(itemID));
                    }
                    break;

                case AdminCommandType.DestroyFirstITem:
                    {
                        // TODO: What does this do?
                    }
                    break;

                case AdminCommandType.GiveExperience:
                    {
                        int amount = iPacket.ReadInt();

                        Experience += amount;
                    }
                    break;

                case AdminCommandType.Ban:
                    {
                        string name = iPacket.ReadString();

                        Character target = null;//this.Client.World.GetCharacter(name);

                        if (target != null)
                        {
                            target.Client.Terminate();
                        }
                        else
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                            {
                                oPacket.WriteByte(6);
                                oPacket.WriteByte(1);

                                Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case AdminCommandType.Block:
                    {
                        // TODO: Ban.
                    }
                    break;

                case AdminCommandType.ShowMessageMap:
                    {
                        // TODO: What does this do?
                    }
                    break;

                case AdminCommandType.Snow:
                    {
                        // TODO: We have yet to implement map weather.
                    }
                    break;

                case AdminCommandType.VarSetGet:
                    {
                        // TODO: This seems useless. Should we implement this?
                    }
                    break;

                case AdminCommandType.Warn:
                    {
                        string name = iPacket.ReadString();
                        string text = iPacket.ReadString();

                        Character target = null;// this.Client.World.GetCharacter(name);

                        if (target != null)
                        {
                            target.Notify(text, NoticeType.Popup);
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                        {
                            oPacket.WriteByte(29);
                            oPacket.WriteBool(target != null);

                            Client.Send(oPacket);
                        }
                    }
                    break;
            }
        }

        public void EnterPortal(PacketReader iPacket)
        {
            byte portals = iPacket.ReadByte();

            if (portals != Portals)
            {
                return;
            }

            string label = iPacket.ReadString();

            Portal portal;

            try
            {
                portal = Map.Portals[label];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            if (false) // TODO: Check if portal is onlyOnce and player already used it.
            {
                // TODO: Send a "closed for now" portal message.

                return;
            }

            try
            {
                new PortalScript(portal, this).Execute();
            }
            catch (Exception ex)
            {
                Log.LogError($"Script error: {ex}");
            }
        }

        public void Report(PacketReader iPacket)
        {
            ReportType type = (ReportType)iPacket.ReadByte();
            string victimName = iPacket.ReadString();
            iPacket.ReadByte(); // NOTE: Unknown.
            string description = iPacket.ReadString();

            ReportResult result;

            switch (type)
            {
                case ReportType.IllegalProgramUsage:
                    {
                    }
                    break;

                case ReportType.ConversationClaim:
                    {
                        string chatLog = iPacket.ReadString();
                    }
                    break;
            }

            if (true) // TODO: Check for available report claims.
            {
                /*if (this.Client.World.IsCharacterOnline(victimName)) // TODO: Should we check for map existance instead? The hacker can teleport away before the reported is executed.
                {
                    if (this.Meso >= 300)
                    {
                        this.Meso -= 300;

                        // TODO: Update GMs of reported player.
                        // TODO: Update available report claims.

                        result = ReportResult.Success;
                    }
                    else
                    {
                        result = ReportResult.UnknownError;
                    }
                }
                else
                {
                    result = ReportResult.UnableToLocate;
                }*/
                result = ReportResult.Success;
            }
            else
            {
                result = ReportResult.Max10TimesADay;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.SueCharacterResult))
            {
                oPacket.WriteByte((byte)result);

                Client.Send(oPacket);
            }
        }

        public byte[] ToByteArray(bool viewAllCharacters = false)
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteBytes(StatisticsToByteArray());
                oPacket.WriteBytes(AppearanceToByteArray());

                if (!viewAllCharacters)
                {
                    oPacket.WriteByte(0); // NOTE: Family
                }

                oPacket.WriteBool(IsRanked);

                if (IsRanked)
                {
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                }

                return oPacket.ToArray();
            }
        }

        public byte[] StatisticsToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {

                oPacket.WriteInt(ID);
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
                oPacket.WriteInt(Map.MapleID);
                oPacket.WriteByte(SpawnPoint);
                oPacket.WriteInt(0);

                return oPacket.ToArray();
            }
        }

        public byte[] AppearanceToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {

                oPacket.WriteByte((int)Gender);
                oPacket.WriteByte(Skin);
                oPacket.WriteInt(Face);
                oPacket.WriteBool(true);
                oPacket.WriteInt(Hair);

                Dictionary<byte, int> visibleLayer = new Dictionary<byte, int>();
                Dictionary<byte, int> hiddenLayer = new Dictionary<byte, int>();

                foreach (Item item in Items.GetEquipped())
                {
                    byte slot = item.AbsoluteSlot;

                    if (slot < 100 && !visibleLayer.ContainsKey(slot))
                    {
                        visibleLayer[slot] = item.MapleID;
                    }
                    else if (slot > 100 && slot != 111)
                    {
                        slot -= 100;

                        if (visibleLayer.ContainsKey(slot))
                        {
                            hiddenLayer[slot] = visibleLayer[slot];
                        }

                        visibleLayer[slot] = item.MapleID;
                    }
                    else if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = item.MapleID;
                    }
                }

                foreach (KeyValuePair<byte, int> entry in visibleLayer)
                {

                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(byte.MaxValue);

                foreach (KeyValuePair<byte, int> entry in hiddenLayer)
                {

                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(byte.MaxValue);

                Item cashWeapon = Items[EquipmentSlot.CashWeapon];

                oPacket.WriteInt(cashWeapon != null ? cashWeapon.MapleID : 0);


                oPacket.WriteInt(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt(0);

                return oPacket.ToArray();
            }
        }

        public byte[] DataToByteArray(long flag = long.MaxValue)
        {
            var pw = new PacketWriter();
            pw.WriteLong(flag);
            pw.WriteByte(0); // NOTE: Unknown.
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteByte(20); // NOTE: Max buddylist size.
            pw.WriteBool(false); // NOTE: Blessing of Fairy.
            pw.WriteInt(Meso);
            pw.WriteBytes(Items.ToByteArray());
            pw.WriteBytes(Skills.ToByteArray());
            pw.WriteBytes(Quests.ToByteArray());
            pw.WriteShort(0);// NOTE: Mini games record.
            pw.WriteShort(0);// NOTE: Rings (1).
            pw.WriteShort(0);// NOTE: Rings (2). 
            pw.WriteShort(0);// NOTE: Rings (3).
            pw.WriteBytes(Trocks.RegularToByteArray());
            pw.WriteBytes(Trocks.VIPToByteArray());
            pw.WriteInt(0); // NOTE: Monster Book cover ID.
            pw.WriteByte(0); // NOTE: Monster Book cards.
            pw.WriteShort(0);// NOTE: New Year Cards.
            pw.WriteShort(0);// NOTE: QuestRecordEX.
            pw.WriteShort(0);// NOTE: AdminShop.
            pw.WriteShort(0); // NOTE: Unknown.
            return pw.ToArray();
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.UserEnterField);

            oPacket.WriteInt(ID);
            oPacket.WriteByte(Level);
            oPacket.WriteString(Name);

            oPacket.WriteString("");
            oPacket.WriteShort(0);
            oPacket.WriteByte(0);
            oPacket.WriteShort(0);
            oPacket.WriteByte(0);


            oPacket.WriteBytes(Buffs.ToByteArray());
            oPacket.WriteShort((short)Job);
            oPacket.WriteBytes(AppearanceToByteArray());
            oPacket.WriteInt(Items.Available(5110000));
            oPacket.WriteInt(0); // NOTE: Item effect.
            oPacket.WriteInt(Item.GetType(Chair) == ItemType.Setup ? Chair : 0);
            oPacket.WriteShort(Position.X);
            oPacket.WriteShort(Position.Y);
            oPacket.WriteByte(Stance);
            oPacket.WriteShort(Foothold);
            oPacket.WriteByte(0);
            oPacket.WriteByte(0);
            oPacket.WriteInt(1);
            oPacket.WriteLong(0);

            if (PlayerShop != null && PlayerShop.Owner == this)
            {

                oPacket.WriteByte(4);
                oPacket.WriteInt(PlayerShop.ObjectID);
                oPacket.WriteString(PlayerShop.Description);
                oPacket.WriteByte(0);
                oPacket.WriteByte(0);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)(PlayerShop.IsFull ? 1 : 2)); // NOTE: Visitor availability.
                oPacket.WriteByte(0);
            }
            else
            {
                oPacket.WriteByte(0);
            }

            bool hasChalkboard = !string.IsNullOrEmpty(Chalkboard);

            oPacket.WriteBool(hasChalkboard);

            if (hasChalkboard)
            {
                oPacket.WriteString(Chalkboard);
            }


            oPacket.WriteByte(0); // NOTE: Couple ring.
            oPacket.WriteByte(0); // NOTE: Friendship ring.
            oPacket.WriteByte(0); // NOTE: Marriage ring.
            oPacket.WriteZeroBytes(3); // NOTE: Unknown.
            oPacket.WriteByte(byte.MaxValue); // NOTE: Team.

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.UserLeaveField);
            oPacket.WriteInt(ID);
            return oPacket;
        }

        internal static bool CharacterExists(string name)
        {
            using (var dbContext = new MapleDbContext())
            {
                return dbContext.Characters.Any(x => x.Name == name);
            }
        }

        internal static void Delete(int characterID)
        {
            using (var dbContext = new MapleDbContext())
            {
                var entity = dbContext.Characters.Find(characterID);
                if (entity != null)
                {
                    dbContext.Characters.Remove(entity);
                }
                dbContext.SaveChanges();
            }
        }
    }
}