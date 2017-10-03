using Microsoft.Extensions.Logging;
using MapleLib.PacketLib;
using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.DB.Models;
using RazzleServer.Inventory;
using RazzleServer.Map;
using RazzleServer.Movement;
using RazzleServer.Party;
using RazzleServer.Scripts;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace RazzleServer.Player
{
    public class MapleCharacter : Character
    {
        private bool _keybindsChanged;
        private bool _quickSlotKeyBindsChanged;
        private readonly List<SpecialPortal> _doors = new List<SpecialPortal>();
        private Dictionary<int, Skill> _skills = new Dictionary<int, Skill>();
        private readonly Dictionary<int, Cooldown> _cooldowns = new Dictionary<int, Cooldown>();
        private readonly Dictionary<int, Buff> _buffs = new Dictionary<int, Buff>();
        private readonly Dictionary<int, MapleSummon> _summons = new Dictionary<int, MapleSummon>();
        private readonly Dictionary<int, MapleQuest> _startedQuests = new Dictionary<int, MapleQuest>();
        private readonly Dictionary<int, uint> _completedQuests = new Dictionary<int, uint>();
        private readonly Dictionary<string, string> _customQuestData = new Dictionary<string, string>();

        private readonly object _hpLock = new object();
        private static readonly object _characterDatabaseLock = new object();
        private static ILogger Log = LogManager.Log;


        public MapleClient Client { get; private set; }
        public Point Position { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public BuffedCharacterStats Stats { get; }
        public Dictionary<InviteType, Invite> Invites = new Dictionary<InviteType, Invite>();
        public bool Hidden { get; set; }
        public SkillMacro[] SkillMacros { get; } = new SkillMacro[5];
        public MapleBuddyList BuddyList { get; set; }
        public Dictionary<uint, Tuple<byte, int>> Keybinds { get; private set; }
        public int[] QuickSlotKeys { get; private set; }
        public List<MapleMovementFragment> LastMove { get; set; }
        public ReactorActionState ReactorActionState { get; set; }
        public MapleMap Map { get; set; }
        public MapleInventory Inventory { get; private set; }
        public MapleGuild Guild { get; set; }
        public MapleTrade Trade { get; set; }
        public MapleParty Party { get; set; }
        public MapleMessengerRoom ChatRoom { get; set; }
        public DateTime LastAttackTime { get; set; }
        public ActionState ActionState { get; private set; } = ActionState.DISABLED;
        public static MapleCharacter GetDefaultCharacter(MapleClient client)
        {
            MapleCharacter newCharacter = new MapleCharacter
            {
                AccountID = client.Account.ID,
                MapID = ServerConfig.Instance.DefaultMapID,
                Level = 1,
                Job = 0,
                Str = 4,
                Dex = 4,
                Luk = 4,
                Int = 4,
                HP = 50,
                MP = 5,
                MaxHP = 50,
                MaxMP = 5,
                Exp = 0,
                AP = 8,
                SP = 0,
                Position = new Point(0, 0),
                Stance = 0,
                Mesos = 0,
                BuddyCapacity = 50
            };

            newCharacter.Inventory = new MapleInventory(newCharacter);

            newCharacter.Stats.Recalculate(newCharacter);

            return newCharacter;
        }

        public MapleCharacter()
        {
            Hidden = false;
            Stats = new BuffedCharacterStats();
            LastAttackTime = DateTime.FromFileTime(0);
        }
        public void LoggedIn()
        {
            //Client.SendPacket(ShowKeybindLayout(Keybinds));
            //Client.SendPacket(ShowQuickSlotKeys(QuickSlotKeys));
            Client.SendPacket(SkillMacro.ShowSkillMacros(SkillMacros));

            Guild = MapleGuild.FindGuild(GuildID);
            Guild?.UpdateGuildData();

            Party = MapleParty.FindParty(ID);
            Party?.UpdateParty();

            BuddyList.NotifyChannelChangeToBuddies(ID, AccountID, Name, Client.Channel, Client, true);
        }

        public void LoggedOut()
        {
            Guild?.UpdateGuildData();
            Party?.CacheCharacterInfo(this);
            Party?.UpdateParty();
            BuddyList.NotifyChannelChangeToBuddies(ID, AccountID, Name, -1);
        }

        public int? GetSavedLocation(string script)
        {
            string data = GetCustomQuestData(CustomQuestKeys.SAVED_LOCATION + script);
            if (string.IsNullOrEmpty(data))
                return null;
            return int.Parse(data);
        }

        public void SaveLocation(string script, int value)
        {
            string data = value.ToString();
            if (value == -1)
                data = null;
            SetCustomQuestData(CustomQuestKeys.SAVED_LOCATION + script, data);
        }

        public void ChangeMap(int mapId, string toPortal = "")
        {
            var map = ServerManager.GetChannelServer(Client.Channel).GetMap(mapId);
            ChangeMap(map, toPortal);
        }

        public void ChangeMap(MapleMap toMap, string toPortal = "", bool fromSpecialPortal = false)
        {
            if (toMap == null) return;
            var portal = string.IsNullOrEmpty(toPortal) ? toMap.GetDefaultSpawnPortal() : toMap.GetPortal(toPortal);
            if (portal == null) return;
            MapleMap oldMap = Map;

            Map = toMap;
            MapID = Map.MapID;

            oldMap.RemoveCharacter(ID);
            Position = portal.Position;
            EnterMap(Client, toMap.MapID, portal.Id, fromSpecialPortal);
            toMap.AddCharacter(this);
            ActionState = ActionState.ENABLED;
        }

        public void Revive(bool returnToCity = false, bool loseExp = true, bool restoreHpToFull = false)
        {
            if (loseExp)
            {
                DoDeathExpLosePenalty();
            }

            HP = 49;
            AddHP(restoreHpToFull ? Stats.MaxHp : (short)1);

            if (returnToCity)
            {
                ChangeMap(Map.ReturnMap);
            }
            else
            {
                EnableActions();
            }
        }

        public void FakeRelog()
        {
            MapleMap currentMap = Map;
            currentMap.RemoveCharacter(ID);
            EnterChannel(Client);
            currentMap.AddCharacter(this);
        }

        public void Release(bool hasMigration = true)
        {
            Inventory?.Release();
            if (Map != null)
            {
                Map.RemoveCharacter(ID);
                Map = null;
            }
            if (ChatRoom != null)
            {
                ChatRoom.RemovePlayer(ID);
                ChatRoom = null;
            }
            if (!hasMigration)
            {
                foreach (Buff buff in _buffs.Values.ToList())
                    buff.CancelRemoveBuffSchedule();
                foreach (MapleSummon summon in _summons.Values.ToList())
                    summon.Dispose();
            }
            Client = null;
        }

        public void SendMessage(string message, byte type = 0)
        {
            Client.SendPacket(ServerNotice(message, type));
        }

        public void SendBlueMessage(string message)
        {
            SendMessage(message, 6);
        }

        public void SendPopUpMessage(string message)
        {
            SendMessage(message, 1);
        }

        public void SendWhiteMessage(string message)
        {
            Client.SendPacket(SystemMessage(message, 0x0B));
        }

        public static bool CharacterExists(string name)
        {
            using (MapleDbContext context = new MapleDbContext())
            {
                return context.Characters.Any(x => x.Name.ToLower().Equals(name.ToLower()));
            }
        }

        public static MapleCharacter ConvertCharacterToMapleCharacter(Character dbChar)
        {
            return new MapleCharacter()
            {
                ID = dbChar.ID,
                Exp = dbChar.Exp,
                Mesos = dbChar.Mesos,
                AccountID = dbChar.AccountID,
                MapID = dbChar.MapID,
                GuildID = dbChar.GuildID,
                HP = dbChar.HP,
                MP = dbChar.MP,
                MaxHP = dbChar.MaxHP,
                MaxMP = dbChar.MaxMP,
                Fame = dbChar.Fame,
                Hair = dbChar.Hair,
                Face = dbChar.Face,
                FaceMark = dbChar.FaceMark,
                GuildContribution = dbChar.GuildContribution,
                Job = dbChar.Job,
                Str = dbChar.Str,
                Dex = dbChar.Dex,
                Int = dbChar.Int,
                Luk = dbChar.Luk,
                AP = dbChar.AP,
                BuddyCapacity = dbChar.BuddyCapacity,
                Level = dbChar.Level,
                Gender = dbChar.Gender,
                Skin = dbChar.Skin,
                SpawnPoint = dbChar.SpawnPoint,
                GuildRank = dbChar.GuildRank,
                AllianceRank = dbChar.AllianceRank,
                Name = dbChar.Name,
                SP = dbChar.SP,
            };
        }

        public bool CreateGuild(string guildName)
        {
            if (Guild != null)
                return false;
            MapleGuild guild = MapleGuild.CreateGuild(guildName, this);
            if (guild == null)
                return false;

            Guild = guild;
            GuildRank = 1;
            GuildContribution = 500;
            AllianceRank = 5;
            SaveToDatabase(this);
            Client.SendPacket(guild.GenerateGuildDataPacket());
            MapleGuild.UpdateCharacterGuild(this, guildName);
            return true;
        }

        public int InsertCharacter()
        {
            Character InsertChar = new Character();
            InsertChar.Name = Name;
            InsertChar.AccountID = AccountID;
            InsertChar.Job = Job;
            InsertChar.Level = Level;
            InsertChar.Str = Str;
            InsertChar.Dex = Dex;
            InsertChar.Int = Int;
            InsertChar.Luk = Luk;
            InsertChar.AP = AP;
            InsertChar.Hair = Hair;
            InsertChar.Face = Face;
            InsertChar.HP = HP;
            InsertChar.MaxHP = MaxHP;
            InsertChar.MP = MP;
            InsertChar.MaxMP = MaxMP;
            InsertChar.Exp = Exp;
            InsertChar.Gender = Gender;
            InsertChar.Skin = Skin;
            InsertChar.FaceMark = FaceMark;
            InsertChar.MapID = ServerConfig.Instance.DefaultMapID;
            InsertChar.BuddyCapacity = 50;
            InsertChar.SP = 0;
            using (MapleDbContext context = new MapleDbContext())
            {
                context.Characters.Add(InsertChar);
                context.SaveChanges();
                ID = InsertChar.ID;

                #region Keybinds
                foreach (var kvp in Keybinds)
                {
                    KeyMap insertKeyMap = new KeyMap();

                    insertKeyMap.CharacterID = ID;
                    insertKeyMap.Key = (byte)kvp.Key; //Posible overflow?
                    insertKeyMap.Type = kvp.Value.Item1;
                    insertKeyMap.Action = kvp.Value.Item2;

                    context.KeyMaps.Add(insertKeyMap);
                }
                for (int i = 0; i < QuickSlotKeys.Length; i++)
                {
                    QuickSlotKeyMap dbQuickSlotKeyMap = new QuickSlotKeyMap();
                    dbQuickSlotKeyMap.CharacterID = InsertChar.ID;
                    dbQuickSlotKeyMap.Key = QuickSlotKeys[i];
                    dbQuickSlotKeyMap.Index = (byte)i;
                    context.QuickSlotKeyMaps.Add(dbQuickSlotKeyMap);
                }
                #endregion

                #region Skills
                foreach (Skill skill in _skills.Values)
                {
                    var insertSkill = new DB.Models.Skill();
                    insertSkill.SkillID = skill.SkillID;
                    insertSkill.CharacterID = ID;
                    insertSkill.Level = skill.Level;
                    insertSkill.MasterLevel = skill.MasterLevel;
                    insertSkill.Expiration = skill.Expiration;

                    context.Skills.Add(insertSkill);
                }
                #endregion
                context.SaveChanges();
            }
            return ID;
        }

        #region Database
        public static MapleCharacter LoadFromDatabase(int characterId, bool characterScreen, MapleClient c = null)
        {
            lock (_characterDatabaseLock)
            {
                using (MapleDbContext dbContext = new MapleDbContext())
                {
                    Character dbChar = dbContext.Characters.SingleOrDefault(x => x.ID == characterId);
                    if (dbChar == null)
                        return null;
                    MapleCharacter chr = ConvertCharacterToMapleCharacter(dbChar);
                    if (c != null)
                    {
                        chr.Client = c;
                    }

                    chr.Inventory = MapleInventory.LoadFromDatabase(chr);

                    foreach (QuestCustomData customQuest in dbContext.QuestCustomData.Where(x => x.CharacterID == characterId))
                    {
                        if (!chr._customQuestData.ContainsKey(customQuest.Key))
                            chr._customQuestData.Add(customQuest.Key, customQuest.Value);
                    }

                    if (characterScreen) return chr; //No need to load more

                    #region Buddies
                    chr.BuddyList = MapleBuddyList.LoadFromDatabase(dbContext.Buddies.Where(x => x.AccountID == chr.AccountID || x.CharacterID == characterId).ToList(), chr.BuddyCapacity);
                    #endregion

                    #region Skills
                    var dbSkills = dbContext.Skills.Where(x => x.CharacterID == characterId);
                    Dictionary<int, Skill> skills = new Dictionary<int, Skill>();
                    foreach (DB.Models.Skill DbSkill in dbSkills)
                    {
                        Skill skill = new Skill(DbSkill.SkillID)
                        {
                            Level = DbSkill.Level,
                            MasterLevel = DbSkill.MasterLevel,
                            Expiration = DbSkill.Expiration,
                            SkillExp = DbSkill.SkillExp
                        };
                        if (!skills.ContainsKey(skill.SkillID))
                            skills.Add(skill.SkillID, skill);
                    }
                    chr.SetSkills(skills);
                    #endregion

                    #region Keybinds
                    List<KeyMap> dbKeyMaps = dbContext.KeyMaps.Where(x => x.CharacterID == characterId).ToList();
                    Dictionary<uint, Tuple<byte, int>> keyMap = new Dictionary<uint, Tuple<byte, int>>();
                    foreach (KeyMap dbKeyMap in dbKeyMaps)
                    {
                        if (!keyMap.ContainsKey(dbKeyMap.Key))
                        {
                            keyMap.Add(dbKeyMap.Key, new Tuple<byte, int>(dbKeyMap.Type, dbKeyMap.Action));
                        }
                    }
                    chr.Keybinds = keyMap;

                    List<QuickSlotKeyMap> dbQuickSlotKeyMaps = dbContext.QuickSlotKeyMaps.Where(x => x.CharacterID == characterId).ToList();
                    int[] quickSlots = new int[28];
                    foreach (QuickSlotKeyMap dbQuickSlotKeyMap in dbQuickSlotKeyMaps)
                    {
                        quickSlots[dbQuickSlotKeyMap.Index] = dbQuickSlotKeyMap.Key;
                    }
                    chr.QuickSlotKeys = quickSlots;

                    List<DbSkillMacro> dbSkillMacros = dbContext.SkillMacros.Where(x => x.CharacterID == characterId).ToList();
                    foreach (DbSkillMacro dbSkillMacro in dbSkillMacros)
                    {
                        chr.SkillMacros[dbSkillMacro.Index] = new SkillMacro(dbSkillMacro.Name, dbSkillMacro.ShoutName, dbSkillMacro.Skill1, dbSkillMacro.Skill2, dbSkillMacro.Skill3);
                    }

                    #endregion

                    #region Cooldowns
                    List<SkillCooldown> DbSkillCooldowns = dbContext.SkillCooldowns.Where(x => x.CharacterID == characterId).ToList();
                    foreach (SkillCooldown DbSkillCooldown in DbSkillCooldowns)
                    {
                        if (!chr._cooldowns.ContainsKey(DbSkillCooldown.SkillID))
                        {
                            DateTime startTime = new DateTime(DbSkillCooldown.StartTime);
                            uint duration = (uint)DbSkillCooldown.Length;
                            uint remaining = (uint)(startTime.AddMilliseconds(duration) - DateTime.UtcNow).TotalMilliseconds;
                            if (remaining <= 2000) // less than 2 seconds is not worth the effort
                                continue;
                            chr.AddCooldownSilent(DbSkillCooldown.SkillID, duration, startTime);
                        }
                    }
                    #endregion

                    #region Quests
                    List<QuestStatus> DbQuestStatuses = dbContext.QuestStatus.Where(x => x.CharacterID == characterId).ToList();
                    foreach (QuestStatus DbQuestStatus in DbQuestStatuses)
                    {
                        MapleQuestStatus status = (MapleQuestStatus)DbQuestStatus.Status;
                        int questId = DbQuestStatus.Quest;
                        if (status == MapleQuestStatus.InProgress)
                        {
                            WzQuest info = DataBuffer.GetQuestById((ushort)questId);
                            if (info != null)
                            {
                                string data = DbQuestStatus.CustomData ?? "";
                                MapleQuest quest = null;
                                if (info.FinishRequirements.Where(x => x.Type == QuestRequirementType.mob).Any())
                                {
                                    List<QuestMobStatus> DbQuestStatusesMobs = dbContext.QuestStatusMobs.Where(x => x.QuestStatusID == DbQuestStatus.ID).ToList();
                                    Dictionary<int, int> mobs = new Dictionary<int, int>();
                                    foreach (QuestMobStatus DbQuestStatusMobs in DbQuestStatusesMobs)
                                    {
                                        int mobId = DbQuestStatusMobs.Mob;
                                        if (mobId > 0)
                                            mobs.Add(mobId, DbQuestStatusMobs.Count);
                                    }
                                    quest = new MapleQuest(info, status, data, mobs);
                                }
                                else
                                {
                                    quest = new MapleQuest(info, status, data);
                                }
                                chr._startedQuests.Add(questId, quest);
                            }
                        }
                        else if (status == MapleQuestStatus.Completed)
                        {
                            if (!chr._completedQuests.ContainsKey(questId))
                                chr._completedQuests.Add(questId, 0x4E35FF7B); //TODO: real date
                        }
                    }
                    #endregion

                    return chr;
                }
            }
        }

        /// <summary>
        /// Saves a character to the database
        /// </summary>
        /// <param name="chr">The character to save</param>
        public static void SaveToDatabase(MapleCharacter chr)
        {
            lock (_characterDatabaseLock)
            {
                using (MapleDbContext dbContext = new MapleDbContext())
                {
                    chr.GuildID = chr.Guild == null ? 0 : chr.Guild.GuildID;

                    if (chr.Map != null)
                    {
                        int forcedReturn = DataBuffer.GetMapById(chr.Map.MapID).ForcedReturn;
                        if (forcedReturn != 999999999)
                        {
                            chr.MapID = forcedReturn;
                            chr.SpawnPoint = 0;
                        }
                        else
                        {
                            chr.MapID = chr.Map.MapID;
                            chr.SpawnPoint = chr.Map.GetClosestSpawnPointId(chr.Position);
                        }
                    }
                    else
                    {
                        chr.SpawnPoint = 0;
                    }

                    Character updateChar = dbContext.Characters.FirstOrDefault(x => x.ID == chr.ID);
                    updateChar.ID = chr.ID;
                    updateChar.Name = chr.Name;
                    updateChar.AccountID = chr.AccountID;
                    updateChar.Job = chr.Job;
                    updateChar.Level = chr.Level;
                    updateChar.Str = chr.Str;
                    updateChar.Dex = chr.Dex;
                    updateChar.Int = chr.Int;
                    updateChar.Luk = chr.Luk;
                    updateChar.AP = chr.AP;
                    updateChar.Hair = chr.Hair;
                    updateChar.Face = chr.Face;
                    updateChar.HP = chr.HP;
                    updateChar.MaxHP = chr.MaxHP;
                    updateChar.MP = chr.MP;
                    updateChar.MaxMP = chr.MaxMP;
                    updateChar.Exp = chr.Exp;
                    updateChar.Mesos = chr.Mesos;
                    updateChar.Gender = chr.Gender;
                    updateChar.Skin = chr.Skin;
                    updateChar.FaceMark = chr.FaceMark;
                    updateChar.MapID = chr.MapID;
                    updateChar.SP = chr.SP;
                    updateChar.BuddyCapacity = chr.BuddyCapacity;

                    chr.Inventory.SaveToDatabase();

                    #region Skills
                    List<DB.Models.Skill> DbSkills = dbContext.Skills.Where(x => x.CharacterID == chr.ID).ToList();
                    foreach (DB.Models.Skill DbSkill in DbSkills)
                    {
                        if (!chr._skills.ContainsKey(DbSkill.SkillID)) //skill was removed                                 
                            dbContext.Skills.Remove(DbSkill);
                    }
                    foreach (Skill skill in chr._skills.Values)
                    {
                        DB.Models.Skill dbSkill = DbSkills.Where(x => x.SkillID == skill.SkillID).FirstOrDefault();
                        if (dbSkill != null) //Update                   
                        {
                            dbSkill.Level = skill.Level;
                            dbSkill.MasterLevel = skill.MasterLevel;
                            dbSkill.Expiration = skill.Expiration;
                            dbSkill.SkillExp = skill.SkillExp;
                        }
                        else //Insert
                        {
                            DB.Models.Skill InsertSkill = new DB.Models.Skill();
                            InsertSkill.CharacterID = chr.ID;
                            InsertSkill.SkillID = skill.SkillID;
                            InsertSkill.Level = skill.Level;
                            InsertSkill.MasterLevel = skill.MasterLevel;
                            InsertSkill.Expiration = skill.Expiration;
                            InsertSkill.SkillExp = skill.SkillExp;
                            dbContext.Skills.Add(InsertSkill);
                        }
                    }
                    #endregion

                    #region Keybinds
                    if (chr._keybindsChanged)
                    {
                        dbContext.KeyMaps.RemoveRange(dbContext.KeyMaps.Where(x => x.CharacterID == chr.ID));
                        foreach (var kvp in chr.Keybinds)
                        {
                            KeyMap insertKeyMap = new KeyMap();
                            insertKeyMap.CharacterID = chr.ID;
                            insertKeyMap.Key = (byte)kvp.Key; //Posible overflow?
                            insertKeyMap.Type = kvp.Value.Item1;
                            insertKeyMap.Action = kvp.Value.Item2;
                            dbContext.KeyMaps.Add(insertKeyMap);
                        }
                    }
                    if (chr._quickSlotKeyBindsChanged)
                    {
                        dbContext.QuickSlotKeyMaps.RemoveRange(dbContext.QuickSlotKeyMaps.Where(x => x.CharacterID == chr.ID));
                        for (int i = 0; i < chr.QuickSlotKeys.Length; i++)
                        {
                            int key = chr.QuickSlotKeys[i];
                            if (key > 0)
                            {
                                QuickSlotKeyMap dbQuickSlotKeyMap = new QuickSlotKeyMap();
                                dbQuickSlotKeyMap.CharacterID = chr.ID;
                                dbQuickSlotKeyMap.Key = key;
                                dbQuickSlotKeyMap.Index = (byte)i;
                                dbContext.QuickSlotKeyMaps.Add(dbQuickSlotKeyMap);
                            }
                        }
                    }
                    List<DbSkillMacro> dbSkillMacros = dbContext.SkillMacros.Where(x => x.CharacterID == chr.ID).ToList();
                    foreach (DbSkillMacro dbSkillMacro in dbSkillMacros)
                    {
                        if (chr.SkillMacros[dbSkillMacro.Index] == null)
                            dbContext.SkillMacros.Remove(dbSkillMacro);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        if (chr.SkillMacros[i] != null && chr.SkillMacros[i].Changed)
                        {
                            SkillMacro macro = chr.SkillMacros[i];
                            DbSkillMacro dbSkillMacro = dbSkillMacros.FirstOrDefault(x => x.Index == i);
                            if (dbSkillMacro != null)
                            {
                                dbSkillMacro.Name = macro.Name;
                                dbSkillMacro.ShoutName = macro.ShoutName;
                                dbSkillMacro.Skill1 = macro.Skills[0];
                                dbSkillMacro.Skill2 = macro.Skills[1];
                                dbSkillMacro.Skill3 = macro.Skills[2];
                            }
                            else
                            {
                                dbSkillMacro = new DbSkillMacro { Index = (byte)i, CharacterID = chr.ID, Name = macro.Name, ShoutName = macro.ShoutName, Skill1 = macro.Skills[0], Skill2 = macro.Skills[1], Skill3 = macro.Skills[2] };
                                dbContext.SkillMacros.Add(dbSkillMacro);
                            }
                            macro.Changed = false;
                        }
                    }
                    #endregion

                    #region Buddies
                    List<MapleBuddy> buddies = chr.BuddyList.GetAllBuddies();
                    var currentDbBuddies = dbContext.Buddies.Where(x => x.AccountID == chr.AccountID || x.CharacterID == chr.ID);
                    //Removed buddies:
                    foreach (Buddy b in currentDbBuddies)
                    {
                        bool accbuddy = b.BuddyAccountID > 0;
                        if (accbuddy)
                        {
                            if (!buddies.Exists(x => x.AccountID == b.BuddyAccountID)) //check if the character's buddlist contains the buddy that is in the database
                                dbContext.Buddies.Remove(b);
                        }
                        else
                        {
                            if (!buddies.Exists(x => x.CharacterID == b.BuddyCharacterID)) //ditto for non-account buddy
                                dbContext.Buddies.Remove(b);
                        }
                    }

                    foreach (MapleBuddy buddy in buddies)
                    {
                        Buddy dbBuddy;
                        if ((dbBuddy = currentDbBuddies.FirstOrDefault(x => x.BuddyAccountID == buddy.AccountID || x.BuddyCharacterID == buddy.CharacterID)) != null)
                        {
                            dbBuddy.Name = buddy.NickName;
                            dbBuddy.Group = buddy.Group;
                            dbBuddy.Memo = buddy.Memo;
                            dbBuddy.IsRequest = buddy.IsRequest;
                        }
                        else
                        {
                            Buddy newBuddy;
                            if (buddy.AccountBuddy)
                            {
                                newBuddy = new Buddy()
                                {
                                    AccountID = chr.AccountID,
                                    BuddyAccountID = buddy.AccountID,
                                    Name = buddy.NickName,
                                    Group = buddy.Group,
                                    Memo = buddy.Memo,
                                    IsRequest = buddy.IsRequest
                                };
                            }
                            else
                            {
                                newBuddy = new Buddy()
                                {
                                    CharacterID = chr.ID,
                                    BuddyCharacterID = buddy.CharacterID,
                                    Name = buddy.NickName,
                                    Group = buddy.Group,
                                    Memo = buddy.Memo,
                                    IsRequest = buddy.IsRequest
                                };
                            }
                            dbContext.Buddies.Add(newBuddy);
                        }
                    }
                    #endregion

                    #region Cooldowns
                    dbContext.SkillCooldowns.RemoveRange(dbContext.SkillCooldowns.Where(x => x.CharacterID == chr.ID));
                    foreach (var kvp in chr._cooldowns)
                    {
                        if (DateTime.UtcNow <= (kvp.Value.StartTime.AddMilliseconds(kvp.Value.Duration)))
                        {
                            SkillCooldown InserSkillCooldown = new SkillCooldown();
                            InserSkillCooldown.CharacterID = chr.ID;
                            InserSkillCooldown.SkillID = kvp.Key;
                            if (kvp.Value.Duration > int.MaxValue)
                                InserSkillCooldown.Length = int.MaxValue;
                            else
                                InserSkillCooldown.Length = (int)kvp.Value.Duration;
                            InserSkillCooldown.StartTime = kvp.Value.StartTime.Ticks;
                            dbContext.SkillCooldowns.Add(InserSkillCooldown);
                        }
                    }
                    #endregion

                    #region Quests
                    var dbCustomQuestData = dbContext.QuestCustomData.Where(x => x.CharacterID == chr.ID).ToList();
                    foreach (var dbCustomQuest in dbCustomQuestData)
                    {
                        if (chr._customQuestData.All(x => x.Key != dbCustomQuest.Key)) //doesn't exist in current chr.CustomQuestData but it does in the DB
                        {
                            dbContext.QuestCustomData.Remove(dbCustomQuest); //Delete it from the DB
                        }
                    }
                    foreach (var kvp in chr._customQuestData)
                    {
                        QuestCustomData dbCustomQuest = dbCustomQuestData.FirstOrDefault(x => x.Key == kvp.Key);
                        if (dbCustomQuest != null)
                        {
                            dbCustomQuest.Value = kvp.Value;
                        }
                        else
                        {
                            QuestCustomData newCustomQuest = new QuestCustomData { CharacterID = chr.ID, Key = kvp.Key, Value = kvp.Value };
                            dbContext.QuestCustomData.Add(newCustomQuest);
                        }
                    }
                    List<QuestStatus> databaseQuests = dbContext.QuestStatus.Where(x => x.CharacterID == chr.ID).ToList();
                    List<QuestStatus> startedDatabaseQuests = databaseQuests.Where(x => x.Status == 1).ToList();
                    List<QuestStatus> completedDatabaseQuests = databaseQuests.Where(x => x.Status == 2).ToList();
                    foreach (QuestStatus qs in startedDatabaseQuests)
                    {
                        if (!chr._startedQuests.ContainsKey(qs.Quest)) //quest in progress was removed or forfeited
                        {
                            dbContext.QuestStatusMobs.RemoveRange(dbContext.QuestStatusMobs.Where(x => x.QuestStatusID == qs.ID));
                            dbContext.QuestStatus.Remove(qs);
                        }
                    }
                    foreach (var questPair in chr._startedQuests)
                    {
                        MapleQuest quest = questPair.Value;
                        QuestStatus dbQuestStatus = startedDatabaseQuests.FirstOrDefault(x => x.Quest == questPair.Key);
                        if (dbQuestStatus != null) //record exists
                        {
                            dbQuestStatus.CustomData = quest.Data;
                            dbQuestStatus.Status = (byte)quest.State;
                            if (quest.HasMonsterKillObjectives)
                            {
                                List<QuestMobStatus> qmsList = dbContext.QuestStatusMobs.Where(x => x.QuestStatusID == dbQuestStatus.ID).ToList();
                                foreach (var mobPair in quest.MonsterKills)
                                {
                                    QuestMobStatus qms = qmsList.FirstOrDefault(x => x.Mob == mobPair.Key);
                                    if (qms != null) //record exists                                
                                        qms.Count = mobPair.Value;
                                    else //doesnt exist yet, need to insert
                                    {
                                        qms = new QuestMobStatus();
                                        qms.Mob = mobPair.Key;
                                        qms.Count = mobPair.Value;
                                        qms.QuestStatusID = dbQuestStatus.ID;
                                        dbContext.QuestStatusMobs.Add(qms);
                                    }
                                }
                            }
                        }
                        else //doesnt exist yet, need to insert
                        {
                            dbQuestStatus = new QuestStatus();
                            dbQuestStatus.CharacterID = chr.ID;
                            dbQuestStatus.Quest = questPair.Key;
                            dbQuestStatus.Status = (byte)quest.State;
                            dbQuestStatus.CustomData = quest.Data;
                            dbContext.QuestStatus.Add(dbQuestStatus);
                            if (quest.HasMonsterKillObjectives)
                            {
                                dbContext.SaveChanges();
                                foreach (var kvp in quest.MonsterKills)
                                {
                                    QuestMobStatus qms = new QuestMobStatus();
                                    qms.QuestStatusID = dbQuestStatus.ID;
                                    qms.Mob = kvp.Key;
                                    qms.Count = kvp.Value;
                                    dbContext.QuestStatusMobs.Add(qms);
                                }
                            }
                        }
                    }
                    foreach (var questPair in chr._completedQuests)
                    {
                        if (!completedDatabaseQuests.Where(x => x.Quest == questPair.Key).Any()) //completed quest isn't in the completed Database yet
                        {
                            QuestStatus qs = databaseQuests.Where(x => x.Quest == questPair.Key).FirstOrDefault();
                            if (qs != null) //quest is in StartedQuests database
                            {
                                dbContext.QuestStatusMobs.RemoveRange(dbContext.QuestStatusMobs.Where(x => x.QuestStatusID == qs.ID));
                                qs.Status = (byte)MapleQuestStatus.Completed;
                                qs.CompleteTime = questPair.Value;
                            }
                            else //not in database yet
                            {
                                qs = new QuestStatus();
                                qs.CharacterID = chr.ID;
                                qs.Quest = questPair.Key;
                                qs.CompleteTime = questPair.Value;
                                qs.Status = (byte)MapleQuestStatus.Completed;
                                dbContext.QuestStatus.Add(qs);
                            }
                        }
                    }
                    #endregion

                    dbContext.SaveChanges();
                }
            }
        }

        #endregion

        public void Bind(MapleClient c)
        {
            Client = c;
            Inventory.Bind(this);
            ActionState = ActionState.ENABLED;
        }

        #region Exp, Level, Job
        public void GainExp(int exp, bool show = false, bool fromMonster = false)
        {
            if (exp == 0 || Level >= 250 || (Level >= 120 && IsCygnus))
                return;
            Exp += exp;
            while (Exp > GameConstants.GetCharacterExpNeeded(Level) && GameConstants.GetCharacterExpNeeded(Level) != 0)
            {
                Exp -= (int)GameConstants.GetCharacterExpNeeded(Level);
                LevelUp();
            }
            UpdateSingleStat(Client, MapleCharacterStat.Exp, Exp, false);
            if (show)
            {
                if (fromMonster)
                {
                    Client.SendPacket(ShowExpFromMonster(exp));
                }
                else
                {
                    //todo: show exp in chat
                }
            }
        }

        public void LevelUp()
        {
            if (Level == 250 || (Level == 120 && IsCygnus))
                return;

            Level++;

            int apIncrease = (IsCygnus && Level <= 70) ? 6 : 5;
            AP += (short)apIncrease;

            #region HpMpIncrease
            int hpInc = 0, mpInc = 0;

            if (IsBeginnerJob)
            {
                hpInc = 13;
                mpInc = 10;
            }
            else if (IsWarrior || IsDawnWarrior || IsMihile)
            {
                hpInc = 66;
                mpInc = 6;
            }
            else if (IsDemonSlayer)
            {
                hpInc = 54;
            }
            else if (IsDemonAvenger)
            {
                hpInc = 30;
            }
            else if (IsHayato)
            {
                hpInc = 46;
                mpInc = 8;
            }
            else if (IsKaiser)
            {
                hpInc = 65;
                mpInc = 5;
            }
            else if (IsMagician || IsBlazeWizard || IsEvan)
            {
                hpInc = IsEvan ? 18 : 12;
                mpInc = 48;
            }
            else if (IsBattleMage)
            {
                hpInc = 36;
                mpInc = 28;
            }
            else if (IsKanna)
            {
                hpInc = 14;
            }
            else if (IsArcher || IsWindArcher || IsMercedes || IsWildHunter || IsThief || IsNightWalker || IsPhantom || IsXenon)
            {
                hpInc = 22;
                mpInc = 15;
            }
            else if (IsPirate || IsJett || IsThunderBreaker || IsMechanic)
            {
                hpInc = 24;
                mpInc = 20;
            }
            else if (IsAngelicBuster)
            {
                hpInc = 30;
            }
            else if (Job >= 900 && Job <= 910)
            {
                hpInc = 500;
                mpInc = 500;
            }
            else
            {
                Log.LogError($"Unhandled Job [{Job}] when giving HP and MP in MapleCharacter.LevelUp()");

            }

            MaxHP += (short)hpInc;
            MaxMP += (short)mpInc;

            MaxHP = (short)Math.Min(500000, Math.Abs(MaxHP));
            MaxMP = (short)Math.Min(500000, Math.Abs(MaxMP));

            //recalc stats

            HP = MaxHP;
            MP = MaxMP;
            #endregion

            Stats.Recalculate(this);
            HP = Stats.MaxHp;
            MP = Stats.MaxMp;
            SP += 3;

            var updatedStats = new SortedDictionary<MapleCharacterStat, int>();
            updatedStats.Add(MapleCharacterStat.Level, Level);
            updatedStats.Add(MapleCharacterStat.Hp, HP);
            updatedStats.Add(MapleCharacterStat.MaxHp, MaxHP);
            updatedStats.Add(MapleCharacterStat.Mp, MP);
            updatedStats.Add(MapleCharacterStat.MaxMp, MaxMP);
            updatedStats.Add(MapleCharacterStat.Ap, AP);
            updatedStats.Add(MapleCharacterStat.Sp, SP);

            if ((IsCygnus || IsMihile) && Level < 120)
            {
                Exp += GameConstants.GetCharacterExpNeeded(Level) / 10;
                updatedStats.Add(MapleCharacterStat.Exp, Exp);
            }

            UpdateStats(Client, updatedStats, false);
            CheckAutoAdvance();
        }

        public void ChangeJob(short newJob)
        {
            if (DataBuffer.GetJobNameById(newJob).Length == 0) //check if valid job
                return;
            Job = newJob;

            var updatedStats = new SortedDictionary<MapleCharacterStat, int> { { MapleCharacterStat.Job, Job } };

            if (newJob % 10 >= 1 && Level >= 70) //3rd job or higher
            {
                AP += 5;
                updatedStats.Add(MapleCharacterStat.Ap, AP);
            }
            int oldMp = MaxMP;
            #region HP increase
            switch (Job)
            {
                #region 1st Job
                case JobConstants.SWORDMAN:
                case JobConstants.DAWNWARRIOR1:
                case JobConstants.HAYATO1:
                case JobConstants.MIHILE1:
                    MaxHP += 269;
                    MaxMP += 10;
                    break;
                case JobConstants.DEMONSLAYER1:
                    MaxHP += 659;
                    break;
                case JobConstants.DEMONAVENGER1:
                    MaxHP += 2209;
                    break;
                case JobConstants.KAISER1:
                    MaxHP += 358;
                    MaxMP += 109;
                    break;
                case JobConstants.MAGICIAN:
                case JobConstants.BLAZEWIZARD1:
                case JobConstants.EVAN1:
                case JobConstants.BEASTTAMER1:
                    MaxHP += 8;
                    MaxMP += 176;
                    break;
                case JobConstants.BATTLEMAGE1:
                    MaxHP += 175;
                    MaxMP += 181;
                    break;
                case JobConstants.KANNA1:
                    MaxHP += 100; //untested
                    break;
                case JobConstants.ARCHER:
                case JobConstants.WINDARCHER1:
                case JobConstants.MERCEDES1:
                case JobConstants.WILDHUNTER1:
                case JobConstants.THIEF:
                case JobConstants.NIGHTWALKER1:
                case JobConstants.PHANTOM1:
                case JobConstants.PIRATE:
                case JobConstants.JETT1:
                case JobConstants.THUNDERBREAKER1:
                case JobConstants.MECHANIC1:
                    MaxHP += 159;
                    MaxMP += 59;
                    break;
                case JobConstants.CANNONEER1:
                    MaxHP += 228;
                    MaxMP += 14;
                    break;
                case JobConstants.XENON1:
                    MaxHP += 459;
                    MaxMP += 159;
                    break;
                #endregion
                #region other jobs
                default:
                    switch (Job / 10)
                    {
                        case 11: //Fighter
                        case 12: //Page
                        case 13: //Dragon Knight
                        case 111: //Dawn Warrior
                        case 211: //Aran
                        case 411: //Hayato
                        case 611: //Kaiser
                        case 1011: //Zero
                            MaxHP += 375;
                            break;
                        case 311: //Demon Slayer
                        case 312: //Demon Avenger
                        case 511: //Mihile
                            MaxHP += 500;
                            break;
                        case 21: //Fire-Poison wizard
                        case 22: //Ice-Lightning wizard
                        case 23: //Cleric
                        case 121: //Blaze Wizard
                        case 1121: //Beast Tamer
                            MaxMP += 480;
                            break;
                        case 221: //Evan         
                            MaxMP += 100;
                            break;
                        case 270: //Luminous
                            MaxMP += 300;
                            break;
                        case 321: //Battle Mage
                            MaxHP += 200;
                            MaxMP += 100;
                            break;
                        case 421: //Kanna
                            MaxHP += 460;
                            break;
                        case 31: //Bowman
                        case 32: //Crossbowman
                        case 41: //Assassin
                        case 42: //Bandit
                        case 51: //Gunslinger
                        case 52: //Brawler
                        case 57: //Jett
                        case 131: //Wind Archer
                        case 141: //Night Walker
                        case 241: //Phantom
                            MaxHP += 310;
                            MaxMP += 160;
                            break;
                        case 231: //Mercedes
                            MaxHP += 170;
                            MaxMP += 160;
                            break;
                        case 331: //Wild Hunter
                            MaxHP += 210;
                            MaxMP += 100;
                            break;
                        case 43: //DualBlade
                        case 361: //Xenon, not checked
                            MaxHP += 175;
                            MaxHP += 100;
                            break;
                        case 53: //Cannoneer
                            MaxHP += 325;
                            MaxHP += 85;
                            break;
                        case 351: //Mechanic
                            MaxHP += 150;
                            MaxMP += 100;
                            break;
                        case 651: //Angelic Burster
                            MaxHP += 350;
                            break;
                        default:
                            if (!IsBeginnerJob)
                            {
                                Log.LogError($"Unhandled Job [{Job}] when giving hp in MapleCharacter.ChangeJob()");
                            }
                            break;
                    }
                    break;
                    #endregion
            }
            HP = MaxHP;
            updatedStats.Add(MapleCharacterStat.MaxHp, MaxHP);
            if (oldMp != MaxMP)
                updatedStats.Add(MapleCharacterStat.MaxMp, MaxMP);
            #endregion

            if (!IsBeginnerJob)
            {
                short sp = 4;
                if ((newJob >= JobConstants.EVAN1 && newJob <= JobConstants.EVAN5) || (IsResistance && newJob % 100 != 0))
                    sp = 3;
                else if (newJob % 100 == 0)
                    sp = 5;
                else
                    sp = 4;

                SP += sp;
            }

            GiveBaseJobSkills(Job);
            Stats.Recalculate(this);
            HP = Stats.MaxHp;
            MP = Stats.MaxMp;
            updatedStats.Add(MapleCharacterStat.Hp, HP);
            updatedStats.Add(MapleCharacterStat.Mp, MP);
            UpdateStats(Client, updatedStats, false);
        }

        public void CheckAutoAdvance()
        {
            int newJob = -1;
            if (IsExplorer || IsCygnus)
            {
                if (IsCygnus && Level >= 30 && Level % 100 == 0)
                {
                    newJob = Job + 10;
                }
                else
                {
                    if (Job % 100 == 0 && Level >= 30)
                    {
                        OpenNpc(1092000);
                        return;
                    }
                    else if ((Job % 10 == 0 && Level >= 60) || (Job % 10 == 1 && Level >= 100))
                        newJob = Job + 1;
                }
            }
            else if (IsEvan)
            {
                if (Job == JobConstants.EVAN1 && Level >= 20)
                    newJob = JobConstants.EVAN2;
                else if ((Job == JobConstants.EVAN2 && Level >= 30) ||
                        (Job == JobConstants.EVAN3 && Level >= 40) ||
                        (Job == JobConstants.EVAN4 && Level >= 50) ||
                        (Job == JobConstants.EVAN5 && Level >= 60) ||
                        (Job == JobConstants.EVAN6 && Level >= 80) ||
                        (Job == JobConstants.EVAN7 && Level >= 100) ||
                        (Job == JobConstants.EVAN8 && Level >= 120) ||
                        (Job == JobConstants.EVAN9 && Level >= 160))
                    newJob = Job + 1;
            }
            else if (IsDemonAvenger)
            {
                if (Job == 3101 && Level >= 30)
                    newJob = 3120;
                else if ((Job == 3120 && Level >= 60) || (Job == 3121 && Level >= 100))
                    newJob = Job + 1;
            }
            else if (IsJett)
            {
                if (Job == 508 && Level >= 30)
                    newJob = 570;
                else if ((Job == 570 && Level >= 60) || (Job == 571 && Level >= 100))
                    newJob = Job + 1;
            }
            else
            {
                if (Job % 100 == 0 && Level >= 30)
                {
                    newJob = Job + 10;
                }
                else if ((Job % 10 == 0 && Level >= 60) || (Job % 10 == 1 && Level >= 100))
                    newJob = Job + 1;
            }

            if (newJob != -1)
                ChangeJob((short)newJob);
        }
        #endregion

        #region Skills, Cooldowns & Keybinds
        public bool HasSkill(int skillId, int skillLevel = 0)
        {
            int trueSkillLevel = GetSkillLevel(skillId);
            if (skillLevel > 0)
                return trueSkillLevel >= skillLevel;
            return trueSkillLevel > 0;
        }

        public byte GetSkillLevel(int skillId)
        {
            int trueSkillId = SkillConstants.CheckAndGetLinkedSkill(skillId);
            Skill skill;
            if (_skills.TryGetValue(trueSkillId, out skill))
                return skill.Level;
            return 0;
        }

        public byte GetSkillMasterLevel(int skillId)
        {
            Skill skill;
            if (_skills.TryGetValue(skillId, out skill))
            {
                return skill.MasterLevel;
            }
            return 0;
        }

        public Skill GetSkill(int skillId)
        {
            Skill skill;
            if (_skills.TryGetValue(skillId, out skill))
            {
                return skill;
            }
            return null;
        }

        public void IncreaseSkillLevel(int skillId, byte amount = 1, bool updateToClient = true)
        {
            Skill skill;
            if (_skills.TryGetValue(skillId, out skill))
            {
                skill.Level += amount;
                if (updateToClient)
                    Client.SendPacket(Skill.UpdateSingleSkill(skill));
                Stats.Recalculate(this);
            }
            else
            {
                LearnSkill(skillId, amount, null, updateToClient);
            }
        }

        public void SetSkillLevel(int skillId, byte level, byte masterLevel = 0, bool updateToClient = true)
        {
            Skill skill;
            if (_skills.TryGetValue(skillId, out skill))
            {
                skill.Level = level;
                if (masterLevel > 0)
                    skill.MasterLevel = masterLevel;
            }
            else
            {
                skill = new Skill(skillId);
                skill.Level = level;
                skill.MasterLevel = masterLevel;
                _skills.Add(skillId, skill);
            }
            if (updateToClient)
                Client.SendPacket(Skill.UpdateSkills(new List<Skill>() { skill }));
            Stats.Recalculate(this);
        }

        public void SetSkillExp(int SkillId, short Exp)
        {
            Skill skill;
            if (_skills.TryGetValue(SkillId, out skill))
                skill.SkillExp = Exp;
        }

        public void SetSkills(Dictionary<int, Skill> skills)
        {
            _skills = skills;
        }

        public void AddSkillSilent(Skill skill)
        {
            if (!_skills.ContainsKey(skill.SkillID))
                _skills.Add(skill.SkillID, skill);
        }

        public void RemoveSkillSilent(int skillId)
        {
            _skills.Remove(skillId);
        }

        public void LearnSkill(int skillId, byte level = 1, WzCharacterSkill skillInfo = null, bool updateToClient = true)
        {
            if (skillInfo == null)
            {
                skillInfo = DataBuffer.GetCharacterSkillById(skillId);
                if (skillInfo == null)
                    return;
            }
            if (!_skills.ContainsKey(skillId))
            {
                Skill skill = new Skill(skillId);
                skill.MasterLevel = skillInfo.HasMastery ? skillInfo.DefaultMastery : skillInfo.MaxLevel;
                skill.Level = level;
                AddSkill(skill);
                if (updateToClient)
                    Client.SendPacket(Skill.UpdateSingleSkill(skill));
                Stats.Recalculate(this);
            }
        }

        public void AddSkills(List<Skill> addSkills, bool updateToClient = true)
        {
            List<Skill> newSkills = new List<Skill>();
            foreach (Skill skill in addSkills)
            {
                if (!_skills.ContainsKey(skill.SkillID))
                {
                    _skills.Add(skill.SkillID, skill);
                    newSkills.Add(skill);
                }
            }
            if (updateToClient) Client.SendPacket(Skill.UpdateSkills(newSkills));
            Stats.Recalculate(this);
        }

        public bool AddSkill(Skill skill)
        {
            if (_skills.ContainsKey(skill.SkillID))
                return false;
            _skills.Add(skill.SkillID, skill);
            return true;
        }

        public void ClearSkills()
        {
            _skills.Clear();
            Stats.Recalculate(this);
        }

        public List<Skill> GetSkillList()
        {
            return _skills.Values.ToList();
        }

        public void GiveBaseJobSkills(int jobId)
        {
            List<WzCharacterSkill> skills = DataBuffer.GetCharacterSkillListByJob(jobId);
            List<Skill> newSkills = new List<Skill>();
            foreach (WzCharacterSkill skillInfo in skills.Where(x => !x.IsInvisible && !x.HasFixedLevel && !x.IsHyperSkill && x.RequiredLevel <= Level && GetSkillLevel(x.SkillId) == 0 && GetSkillMasterLevel(x.SkillId) == 0 && x.DefaultMastery > 0))
            {
                Skill skill = new Skill(skillInfo.SkillId);
                skill.MasterLevel = skillInfo.DefaultMastery;
                newSkills.Add(skill);
            }

            AddSkills(newSkills);
        }

        public void AddCooldown(int skillId, uint duration, DateTime? nStartTime = null) //duration in MS
        {
            DateTime startTime = nStartTime ?? DateTime.UtcNow;
            if (!_cooldowns.ContainsKey(skillId))
            {
                Cooldown cd = new Cooldown(duration, startTime);
                cd.CancellationToken = new CancellationTokenSource();
                Scheduler.ScheduleRemoveCooldown(this, skillId, (int)duration, cd.CancellationToken.Token);
                _cooldowns.Add(skillId, cd);
                Client.SendPacket(Skill.ShowCooldown(skillId, duration / 1000));
            }
        }

        public void AddCooldownSilent(int skillId, uint duration, DateTime? nStartTime = null, bool createCancelSchedule = true) //duration in MS
        {
            DateTime startTime = nStartTime ?? DateTime.UtcNow;
            if (!_cooldowns.ContainsKey(skillId))
            {
                Cooldown cd = new Cooldown(duration, startTime);
                cd.CancellationToken = new CancellationTokenSource();
                if (createCancelSchedule)
                    Scheduler.ScheduleRemoveCooldown(this, skillId, (int)duration, cd.CancellationToken.Token);
                _cooldowns.Add(skillId, cd);
            }
        }


        public bool HasSkillOnCooldown(int skillId)
        {
            Cooldown cooldown;
            if (_cooldowns.TryGetValue(skillId, out cooldown))
            {
                if (cooldown.StartTime.AddMilliseconds(cooldown.Duration) > DateTime.UtcNow)
                    return true;
                else
                {
                    _cooldowns.Remove(skillId);
                    return false;
                }
            }
            else
                return false;
        }

        public void RemoveCooldown(int skillId)
        {
            if (_cooldowns.Remove(skillId))
                Client.SendPacket(Skill.ShowCooldown(skillId, 0));
        }

        public void ChangeKeybind(uint key, byte type, int action)
        {
            Keybinds.Remove(key);
            if (type != 0)
                Keybinds.Add(key, new Tuple<byte, int>(type, action));
            _keybindsChanged = true;
        }

        public void SetQuickSlotKeys(int[] newMap)
        {
            QuickSlotKeys = newMap;
            _quickSlotKeyBindsChanged = true;
        }

        public void SetKeyMap(Dictionary<uint, Tuple<byte, int>> newBinds)
        {
            Keybinds = newBinds;
            _keybindsChanged = true;
        }
        #endregion

        #region Buffs
        public void GiveBuff(Buff buff)
        {
            CancelBuffSilent(buff.SkillId);
            _buffs.Add(buff.SkillId, buff);
            switch (buff.SkillId)
            {
                case Spearman.EVIL_EYE:
                case Berserker.EVIL_EYE_OF_DOMINATION:
                    Client.SendPacket(Buff.GiveEvilEyeBuff(buff));
                    break;
                case Berserker.CROSS_SURGE:
                    Client.SendPacket(Buff.GiveCrossSurgeBuff(buff, this, buff.Effect));
                    break;
                case DarkKnight.FINAL_PACT2:
                    Client.SendPacket(Buff.GiveFinalPactBuff(buff));
                    break;
                case Priest.HOLY_MAGIC_SHELL:
                    buff.Stacks = buff.Effect.Info[CharacterSkillStat.x];
                    AddCooldownSilent(Priest.HOLY_MAGIC_SHELL + 1000, (uint)buff.Duration, buff.StartTime, false); //hackish
                    Client.SendPacket(Buff.GiveBuff(buff));
                    break;
                default:
                    Client.SendPacket(Buff.GiveBuff(buff));
                    //TODO: broadcast to map
                    break;
            }
            Stats.Recalculate(this);
        }

        public void GiveBuffSilent(Buff buff) //Doesn't send the buff packet to the player
        {
            int skillId = buff.SkillId;
            CancelBuffSilent(skillId);
            _buffs.Add(skillId, buff);
            Stats.Recalculate(this);
        }

        public Buff CancelBuff(int skillId)
        {
            Buff buff;
            if (!_buffs.TryGetValue(skillId, out buff)) return null;
            _buffs.Remove(skillId);
            buff.CancelRemoveBuffSchedule();
            Client.SendPacket(Buff.CancelBuff(buff));
            Stats.Recalculate(this);

            #region Dark Knight Final Pact
            if (buff.SkillId == DarkKnight.FINAL_PACT2)
            {
                if (buff.Stacks > 0) //didn't kill enough mobs
                {
                    AddHP((short)-HP);
                }
            }
            #endregion

            return buff;
        }

        public void CancelBuffs(List<int> skillIds)
        {
            bool removed = false;
            foreach (int i in skillIds)
            {
                Buff buff;
                if (!_buffs.TryGetValue(i, out buff)) continue;
                removed = true;
                _buffs.Remove(i);
                buff.CancelRemoveBuffSchedule();
                Client.SendPacket(Buff.CancelBuff(buff));
            }
            if (removed)
                Stats.Recalculate(this);
        }

        public Buff CancelBuffSilent(int skillId) //doesnt update client and doesnt recalculate stats
        {
            Buff buff;
            if (!_buffs.TryGetValue(skillId, out buff)) return null;
            _buffs.Remove(skillId);
            buff.CancelRemoveBuffSchedule();
            return buff;
        }

        public void CancelBuffsSilent(List<int> skillIds) //doesnt update client and doesnt recalculate stats
        {
            foreach (int i in skillIds)
            {
                Buff buff;
                if (!_buffs.TryGetValue(i, out buff)) continue;
                _buffs.Remove(i);
                buff.CancelRemoveBuffSchedule();
            }
        }

        public bool HasBuff(int skillId)
        {
            return _buffs.ContainsKey(skillId);
        }

        public bool HasBuffStat(BuffStat buffStat)
        {
            foreach (Buff buff in _buffs.Values.ToList())
            {
                if (buff.Effect.BuffInfo.ContainsKey(buffStat))
                {
                    return true;
                }
            }
            return false;
        }

        public Buff GetBuff(int skillId)
        {
            Buff ret;
            if (_buffs.TryGetValue(skillId, out ret))
                return ret;
            return null;
        }

        public List<Buff> GetBuffs()
        {
            return _buffs.Values.ToList();
        }
        #endregion

        #region Summons
        public bool HasActiveSummon(int sourceSkillId)
        {
            return _summons.ContainsKey(sourceSkillId);
        }

        public void AddSummon(MapleSummon summon)
        {
            _summons.Add(summon.SourceSkillId, summon);
            Map.AddSummon(summon, true);
        }

        public bool RemoveSummon(int skillId)
        {
            MapleSummon summon;
            if (!_summons.TryGetValue(skillId, out summon)) return false;
            _summons.Remove(summon.SourceSkillId);
            Map.RemoveSummon(summon.ObjectID, true);
            summon.Dispose();
            CancelBuff(skillId);
            return true;
        }

        public MapleSummon GetSummon(int skillId)
        {
            MapleSummon ret;
            return _summons.TryGetValue(skillId, out ret) ? ret : null;
        }

        public List<MapleSummon> GetSummons()
        {
            return _summons.Values.ToList();
        }
        #endregion

        #region Quests
        public MapleQuest ForfeitQuest(int questId)
        {
            MapleQuest quest = null;
            if (_startedQuests.TryGetValue(questId, out quest))
            {
                _startedQuests.Remove(questId);
                quest.Forfeit();
                Client.SendPacket(quest.Update());
            }
            return quest;
        }

        public bool CompleteQuest(int questId, int npcId, int choice = 0)
        {
            MapleQuest quest = null;
            if (_startedQuests.TryGetValue(questId, out quest))
            {
                if (Map == null || !Map.HasNpc(npcId)) return false;
                foreach (WzQuestRequirement wqr in quest.QuestInfo.FinishRequirements)
                {
                    if (!wqr.Check(this, npcId, quest))
                        return false;
                }
                ForceCompleteQuest(quest, npcId);
                foreach (WzQuestAction wqa in quest.QuestInfo.FinishActions)
                {
                    wqa.Act(this, questId);
                }
                return true;
            }
            return false;
        }

        public bool HasCompletedQuest(int questId)
        {
            return _completedQuests.ContainsKey(questId);
        }

        public int CompletedQuestCount
        {
            get
            {
                return _completedQuests.Count;
            }
        }

        public bool HasQuestInProgress(int questId)
        {
            return _startedQuests.ContainsKey(questId);
        }

        public MapleQuest GetQuest(ushort questId)
        {
            MapleQuest ret;
            if (_startedQuests.TryGetValue(questId, out ret))
                return ret;
            return null;
        }

        public bool StartQuest(int questId, int npcId)
        {
            if (!_startedQuests.ContainsKey(questId) && !_completedQuests.ContainsKey(questId))
            {
                if (Map == null || !Map.HasNpc(npcId)) return false;
                WzQuest info = DataBuffer.GetQuestById((ushort)questId);
                if (info == null) return false;
                MapleQuest quest = new MapleQuest(info);
                foreach (WzQuestRequirement wqr in info.StartRequirements)
                {
                    if (!wqr.Check(this, npcId, quest))
                        return false;
                }
                foreach (WzQuestAction wqa in quest.QuestInfo.StartActions)
                {
                    wqa.Act(this, questId);
                }
                _startedQuests.Add(questId, quest);
                Client.SendPacket(quest.Update());
            }
            return false;
        }

        public void AddQuest(MapleQuest quest, ushort questId)
        {
            if (!_startedQuests.ContainsKey(questId))
            {
                _startedQuests.Add(questId, quest);
                Client.SendPacket(quest.Update());
            }
        }

        public bool ForceCompleteQuest(MapleQuest quest, int npcId, int nextQuest = 0)
        {
            int questId = quest.QuestInfo.Id;
            if (_startedQuests.ContainsKey(questId))
            {
                _startedQuests.Remove(questId);
                quest.State = MapleQuestStatus.Completed;
                if (!_completedQuests.ContainsKey(questId))
                    _completedQuests.Add(questId, 0x4E35FF7B); //TODO: real date, some korean thing in minutes I think
                Client.SendPacket(quest.Update());
                Client.SendPacket(quest.UpdateFinish(npcId, nextQuest));
                return true;
            }
            else if (!_completedQuests.ContainsKey(questId))
            {
                _completedQuests.Add(questId, 0x4E35FF7B); //TODO: real date, some korean thing in minutes I think
                Client.SendPacket(quest.Update());
                Client.SendPacket(quest.UpdateFinish(npcId, nextQuest));
            }
            return false;
        }

        public void UpdateQuestKills(int mobId)
        {
            foreach (var quest in _startedQuests)
                quest.Value.KilledMob(Client, mobId);
        }

        public void SetQuestData(ushort questId, string data)
        {
            MapleQuest quest;
            if (_startedQuests.TryGetValue(questId, out quest))
            {
                quest.Data = data;
            }
            else
            {
                WzQuest info = DataBuffer.GetQuestById(questId);
                if (info == null) return;
                quest = new MapleQuest(info, MapleQuestStatus.InProgress, data);
                _startedQuests.Add(questId, quest);
            }
        }

        public string GetQuestData(ushort questId)
        {
            MapleQuest quest;
            return _startedQuests.TryGetValue(questId, out quest) ? quest.Data : null;
        }

        public string GetCustomQuestData(string customQuestKey)
        {
            string data;
            return _customQuestData.TryGetValue(customQuestKey, out data) ? data : string.Empty;
        }

        //Removes the custom quest from the collection if data is null or empty
        public void SetCustomQuestData(string customQuestKey, string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _customQuestData.Remove(customQuestKey);
                return;
            }
            if (_customQuestData.ContainsKey(customQuestKey))
                _customQuestData[customQuestKey] = data;
            else
                _customQuestData.Add(customQuestKey, data);
        }
        #endregion

        #region Doors
        public bool HasDoor(int skillId)
        {
            lock (_doors)
            {
                List<SpecialPortal> sameSkillPortals = _doors.Where(x => x.SkillId == skillId).ToList();
                int count = sameSkillPortals.Count;
                foreach (SpecialPortal portal in sameSkillPortals)
                {
                    if (DateTime.UtcNow >= portal.Expiration)
                    {
                        portal.FromMap.RemoveStaticObject(portal.ObjectID, false);
                        _doors.Remove(portal);
                        count--;
                    }
                }
                return count > 0;
            }
        }

        public void CancelDoor(int skillId = 0)
        {
            List<SpecialPortal> sameSkillDoors;
            lock (_doors)
            {
                sameSkillDoors = _doors.Where(x => skillId == 0 ? true : x.SkillId == skillId).ToList();
            }
            foreach (SpecialPortal door in sameSkillDoors)
            {
                door.FromMap.RemoveStaticObject(door.ObjectID, false);
            }
        }

        public void RemoveDoor(int skillId)
        {
            lock (_doors)
            {
                _doors.RemoveAll(x => x.SkillId == skillId);
            }
        }

        public void AddDoor(SpecialPortal door)
        {
            lock (_doors)
            {
                _doors.Add(door);
            }
        }
        #endregion

        #region Stats
        public void AddHP(int hpInc, bool updateToClient = true, bool healIfDead = false)
        {
            lock (_hpLock)
            {
                if (IsDead && !healIfDead) return;

                short oldHp = HP;
                short newHp = (short)(HP + hpInc);
                if (newHp > Stats.MaxHp)
                    HP = Stats.MaxHp;
                else if (newHp <= 0)
                {
                    HP = 0;
                    if (oldHp > HP)
                        HandleDeath();
                }
                else
                    HP = newHp;

                if (Job == JobConstants.DARKKNIGHT)
                    FinalPactHook(oldHp, newHp);

                if (updateToClient)
                    UpdateSingleStat(Client, MapleCharacterStat.Hp, HP);

                if (Party != null)
                {
                    var members = Party.GetCharactersOnMap(Map, ID);
                    if (members.Any())
                    {
                        PacketWriter partyHpUpdatePacket = MapleParty.Packets.UpdatePartyMemberHp(this);
                        foreach (MapleCharacter member in members)
                        {
                            member.Client.SendPacket(partyHpUpdatePacket);
                        }
                    }
                }
            }
        }

        public void HandleDeath()
        {
            ActionState = ActionState.DEAD;
            if (Job == JobConstants.DARKKNIGHT && !_cooldowns.ContainsKey(DarkKnight.FINAL_PACT2))
            {
                byte skillLevel = GetSkillLevel(DarkKnight.FINAL_PACT);
                if (skillLevel > 0)
                {
                    SkillEffect effect = DataBuffer.GetCharacterSkillById(DarkKnight.FINAL_PACT2).GetEffect(skillLevel);
                    AddCooldown(DarkKnight.FINAL_PACT2, (uint)effect.Info[CharacterSkillStat.cooltime] * 1000);
                    Buff buff = new Buff(DarkKnight.FINAL_PACT2, effect, effect.Info[CharacterSkillStat.time] * 1000, this);
                    buff.Stacks = effect.Info[CharacterSkillStat.z];
                    GiveBuff(buff);
                    Client.SendPacket(Skill.ShowBuffEffect(DarkKnight.FINAL_PACT2, Level, null, true));
                    ActionState = ActionState.ENABLED;
                    HP = Stats.MaxHp;
                    AddMP(Stats.MaxMp);
                    return;
                }
            }
            foreach (Buff buff in _buffs.Values.ToList())
            {
                CancelBuffSilent(buff.SkillId);
            }
            foreach (MapleSummon summon in _summons.Values.ToList())
            {
                RemoveSummon(summon.SourceSkillId);
            }
            Stats.Recalculate(this);
        }

        public void AddMP(int mpInc, bool updateToClient = true)
        {
            short buffedMaxMP = Stats.MaxMp;
            if (MP + mpInc > buffedMaxMP)
                MP = buffedMaxMP;
            else if (MP + mpInc < 0)
                MP = 0;
            else
                MP += (short)mpInc;
            if (updateToClient)
                UpdateSingleStat(Client, MapleCharacterStat.Mp, MP, true);
        }

        private void FinalPactHook(int oldHp, int newHp)
        {
            byte skillLevel = GetSkillLevel(DarkKnight.FINAL_PACT);
            if (skillLevel > 0)
            {
                SkillEffect passiveEffect = DataBuffer.GetCharacterSkillById(DarkKnight.FINAL_PACT).GetEffect(skillLevel);
                int boundary = passiveEffect.Info[CharacterSkillStat.x];
                int oldPercent = (int)((oldHp / (double)Stats.MaxHp) * 100);
                int newPercent = (int)((newHp / (double)Stats.MaxHp) * 100);
                if (oldPercent < boundary && newPercent >= boundary) //hp went from under to above the boundary
                    Client.SendPacket(Skill.ShowBuffEffect(DarkKnight.FINAL_PACT, Level, skillLevel, true)); //TODO: show other ppl (dont know packet)
                else if (oldPercent >= boundary && newPercent < boundary) //hp went from above to under the boundary
                    Client.SendPacket(Skill.ShowBuffEffect(DarkKnight.FINAL_PACT, Level, skillLevel, false)); //TODO: show other ppl (dont know packet)               
            }
        }

        public void DoDeathExpLosePenalty()
        {
            int totalNeededExp = GameConstants.GetCharacterExpNeeded(Level);
            int loss = (int)(totalNeededExp * 0.1);
            if (Stats.ExpLossReductionR > 0)
                loss -= (int)(loss * (Stats.ExpLossReductionR / 100.0));
            Exp -= loss;
            Exp = Math.Max(0, Exp);
            UpdateSingleStat(Client, MapleCharacterStat.Exp, Exp);
        }

        public void AddFame(int amount)
        {
            Fame += (short)amount;
            UpdateSingleStat(Client, MapleCharacterStat.Fame, amount);
        }
        #endregion

        #region NPC
        public void OpenNpc(int npcId)
        {
            Client.NpcEngine?.Dispose();
            NpcEngine.OpenNpc(npcId, -1, Client);
        }
        #endregion

        #region Packets
        public static PacketWriter ShowExpFromMonster(int exp)
        {
            var pw = new PacketWriter(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteByte(3);
            pw.WriteByte(1);
            pw.WriteInt(exp);
            pw.WriteZeroBytes(9);
            return pw;
        }

        public static PacketWriter ShowGainMapleCharacterStat(int amount, MapleCharacterStat stat)
        {
            var pw = new PacketWriter(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteByte(0x11);
            pw.WriteLong((long)stat);
            pw.WriteInt(amount);
            return pw;
        }


        public static void AddCharEntry(PacketWriter pw, MapleCharacter chr, bool viewAll = false)
        {
            AddCharStats(pw, chr);
            AddCharLook(pw, chr, true);

            if (!viewAll)
            {
                pw.WriteByte(0);
            }

            bool rankEnabled = false;
            pw.WriteBool(rankEnabled);
            if (rankEnabled)
            {
                pw.WriteInt(0); // Rank
                pw.WriteInt(0); // Rank Move
                pw.WriteInt(0); // Job Rank
                pw.WriteInt(0); // Job Rank Move       
            }
        }

        public static void AddCharStats(PacketWriter pw, MapleCharacter chr, bool CashShop = false)
        {
            pw.WriteInt(chr.ID);
            pw.WriteStaticString(chr.Name, 13);
            pw.WriteByte(chr.Gender);
            pw.WriteByte(chr.Skin);
            pw.WriteInt(chr.Face);
            pw.WriteInt(chr.Hair);

            for (int i = 0; i < 3; i++)
            {
                pw.WriteLong(0); // PetID
            }

            pw.WriteByte(chr.Level);
            pw.WriteShort(chr.Job);

            pw.WriteShort(chr.Str);
            pw.WriteShort(chr.Dex);
            pw.WriteShort(chr.Int);
            pw.WriteShort(chr.Luk);

            pw.WriteShort(chr.HP);
            pw.WriteShort(chr.MaxHP);
            pw.WriteShort(chr.MP);
            pw.WriteShort(chr.MaxMP);

            pw.WriteShort(chr.AP);
            pw.WriteShort(chr.SP);

            pw.WriteInt(chr.Exp);
            pw.WriteShort(chr.Fame);

            pw.WriteInt(0); // Gachapon EXP
            pw.WriteInt(chr.MapID);
            pw.WriteByte(chr.SpawnPoint);
            pw.WriteInt(0);
        }

        public static void AddCharLook(PacketWriter pw, MapleCharacter chr, bool mega = false)
        {
            pw.WriteByte(chr.Gender);
            pw.WriteByte(chr.Skin);
            pw.WriteInt(chr.Face);
            pw.WriteBool(mega);
            pw.WriteInt(chr.Hair);
            AddEquipInfo(pw, chr);
        }

        private static void AddEquipInfo(PacketWriter pw, MapleCharacter chr)
        {
            IEnumerable<MapleItem> allEquips = chr.Inventory.GetItemsFromInventory(MapleInventoryType.Equipped);
            Dictionary<byte, MapleItem> equips = new Dictionary<byte, MapleItem>();
            Dictionary<byte, MapleItem> maskedEquips = new Dictionary<byte, MapleItem>();
            foreach (MapleItem equip in allEquips)
            {
                byte pos = (byte)Math.Abs(equip.Position);
                if (!equips.ContainsKey(pos) && pos < 100)
                    equips.Add(pos, equip);
                else if (pos > 100 && pos != 111)
                {
                    pos %= 100;
                    if (equips.ContainsKey(pos))
                        maskedEquips.Add(pos, equip);
                    else
                        equips.Add(pos, equip);
                }
                else if (equips.ContainsKey(pos))
                    maskedEquips.Add(pos, equip);
            }

            foreach (var equip in equips)
            {
                pw.WriteByte(equip.Key);
                pw.WriteInt(equip.Value.ItemId);
            }
            pw.WriteByte(0xFF);

            foreach (var equip in maskedEquips)
            {
                pw.WriteByte(equip.Key);
                pw.WriteInt(equip.Value.ItemId);
            }
            pw.WriteByte(0xFF);

            MapleItem weapon = chr.Inventory.GetEquippedItem((short)MapleEquipPosition.Weapon);
            int weaponId = 0;
            if (weapon != null) weaponId = weapon.ItemId;
            pw.WriteInt(weaponId);

            for (int i = 0; i < 3; i++)
            {
                pw.WriteInt(0); // Pet ID
            }
        }

        public static void AddMonsterBookInfo(PacketWriter pw, MapleCharacter chr)
        {
            pw.WriteInt(0); // cover TODO???
            pw.WriteByte(0);

            var cards = new object[0];
            pw.WriteShort((short)cards.Length);
            // for each card
            //pw.WriteShort(key % 10000); // id
            //pw.WriteByte(value); // level
        }

        public static void AddCharInfo(PacketWriter pw, MapleCharacter chr)
        {
            pw.WriteHexString("FF FF FF FF FF FF FF FF");

            pw.WriteByte(0);

            pw.WriteUInt(0xFFFFFFF8); //new v135 FA FF FF FF in v158?
            pw.WriteUInt(0xFFFFFFF8); //new v135
            pw.WriteUInt(0xFFFFFFF8); //new v135

            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteInt(0);
            pw.WriteByte(0);

            AddCharStats(pw, chr);

            pw.WriteByte(50); //buddy list capacity

            pw.WriteByte(0);

            pw.WriteByte(0);


            //MapleInventory.Packets.AddInventoryInfo(pw, chr.Inventory);

            //SkillInfo
            pw.WriteByte(1); //use old, max 500 skills            

            //pw.WriteShort((short)chr.Skills.Count);
            pw.WriteShort(0);

            // foreach (KeyValuePair<int, Skill> kvp in chr.Skills)
            // {
            //     if (kvp.Value.SkillExp != 0)
            //     {
            //         pw.WriteInt(kvp.Key);
            //         pw.WriteShort(kvp.Value.SkillExp);
            //         pw.WriteByte(0);
            //         pw.WriteByte(kvp.Value.Level);
            //         pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(kvp.Value.Expiration));
            //     }
            //     else
            //     {
            //         pw.WriteInt(kvp.Key);
            //         pw.WriteInt(kvp.Value.Level);
            //         pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(kvp.Value.Expiration));
            //         if (kvp.Value.HasMastery) //hyper skills as well
            //             pw.WriteInt(kvp.Value.MasterLevel);
            //     }
            // }

            pw.WriteShort(0);

            //Cooldowninfo
            // pw.WriteShort((short)chr.Cooldowns.Count); //cooldown size
            // foreach (var kvp in chr.Cooldowns)
            // {
            //     pw.WriteInt(kvp.Key); //skill Id
            //     int remaining = (int)(kvp.Value.StartTime.AddMilliseconds(kvp.Value.Duration) - DateTime.UtcNow).TotalMilliseconds;
            //     pw.WriteInt(remaining / 1000); //cooldown time is in seconds
            // }

            //Quests
            // pw.WriteByte(1);
            // pw.WriteUShort((ushort)chr.StartedQuests.Count); //started quests size
            // foreach (var questPair in chr.StartedQuests)
            // {
            //     pw.WriteUShort((ushort)questPair.Key);
            //     pw.WriteMapleString(questPair.Value.Data);
            // }
            // pw.WriteShort(0); //some custom string quests or something, each one has 2 maplestrings, 2 examples: "1NX5211068" "1", "SE20130116" "1"
            // pw.WriteByte(1);
            // pw.WriteUShort((ushort)chr.CompletedQuests.Count); //completed quests size
            // foreach (var questPair in chr.CompletedQuests)
            // {
            //     pw.WriteUShort((ushort)questPair.Key);
            //     pw.WriteUInt(questPair.Value);
            // }





            // {   //TODO: monster cards
            //     pw.WriteInt(0);
            //     pw.WriteByte(0); //unfinished
            //     pw.WriteShort(0); //size of card list
            //     pw.WriteInt(-1); //current set selected for bonus
            // }

            pw.WriteShort(0);
            pw.WriteShort(0);


            { //custom questinfo

                pw.WriteShort(0); //size
                                  //foreach:
                                  //pw.WriteShort(key);
                                  //pw.WriteMapleString(questinfo); e.g.: "RG=0;SM=0;ALP=0;DB=0;CD=0;MH=0"
            }


            pw.WriteShort(0); //dunno, probably amount for something
            pw.WriteShort(0); //dunno, probably amount for something

            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(-1);
            pw.WriteInt(0);


            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2));

            pw.WriteShort(0); //dunno
            pw.WriteShort(0); //dunno
            pw.WriteInt(0); //dunno

            pw.WriteInt(chr.AccountID);
            pw.WriteInt(chr.ID);

            int size = 4;
            pw.WriteInt(size);
            for (int i = 0; i < size; i++)
                pw.WriteLong(9410165 + i);

        }

        public static void EnterChannel(MapleClient c)
        {
            MapleCharacter chr = c.Account.Character;
            PacketWriter pw = new PacketWriter(SMSGHeader.ENTER_MAP);
            pw.WriteInt(c.Channel);
            pw.WriteByte(1);
            pw.WriteByte(1);
            pw.WriteShort(0); //1 = 10 sec login message (2x maplestring credz -> Clarity)
            for (var i = 0; i < 3; i++)
            {
                pw.WriteInt(Functions.Random());
            }

            AddCharacterInfo(chr, pw);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow)); //current time
            c.SendPacket(pw);
        }

        private static void AddCharacterInfo(MapleCharacter chr, PacketWriter pw)
        {
            pw.WriteLong(-1);
            pw.WriteByte(0);

            AddCharStats(pw, chr);
            pw.WriteByte(chr.BuddyCapacity);

            string linkedName = null;
            pw.WriteBool(linkedName != null);
            if (linkedName != null)
            {
                pw.WriteMapleString(linkedName);
            }

            pw.WriteInt(chr.Mesos);
            AddInventoryInfo(pw, chr);
            AddSkillInfo(pw, chr);
            AddQuestInfo(pw, chr);
            pw.WriteShort(0);
            AddRingInfo(pw);
            AddTeleportRockInfo(pw);
            AddMonsterBookInfo(pw, chr);
            pw.WriteShort(0);
            pw.WriteInt(0);
        }

        private static void AddTeleportRockInfo(PacketWriter pw)
        {
            for (int x = 0; x < 15; x++)
            {
                // 5 - TELEPORT ROCK
                // 10 - VIP TELEPORT ROCK
                // CHAR INFO MAGIC
                pw.WriteBytes(new byte[] { 0xFF, 0xC9, 0x9A, 0x3B });
            }
        }

        private static void AddRingInfo(PacketWriter pw)
        {
            pw.WriteShort(0); // crush rings
            pw.WriteShort(0); // friendship rings
            pw.WriteShort(0); // marriage rings

        }

        private static void AddQuestInfo(PacketWriter pw, MapleCharacter chr)
        {
            pw.WriteUShort((ushort)chr._startedQuests.Count); //started quests size
            foreach (var questPair in chr._startedQuests)
            {
                pw.WriteUShort((ushort)questPair.Key);
                pw.WriteMapleString(questPair.Value.Data);
            }
            pw.WriteUShort((ushort)chr._completedQuests.Count); //completed quests size
            foreach (var questPair in chr._completedQuests)
            {
                pw.WriteUShort((ushort)questPair.Key);
                pw.WriteLong(questPair.Value);
            }
        }

        private static void AddSkillInfo(PacketWriter pw, MapleCharacter chr)
        {
            pw.WriteByte(0);
            pw.WriteShort((short)chr._skills.Count);
            foreach (var skill in chr._skills)
            {
                pw.WriteInt(skill.Key);
                pw.WriteInt(skill.Value.Level);
                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(skill.Value.Expiration));

                if (skill.Value.HasMastery)
                {
                    pw.WriteInt(skill.Value.MasterLevel);
                }
            }

            pw.WriteShort((short)chr._cooldowns.Count);
            foreach (var cooldown in chr._cooldowns)
            {
                pw.WriteInt(cooldown.Key);
                int remaining = (int)(cooldown.Value.StartTime.AddMilliseconds(cooldown.Value.Duration) - DateTime.UtcNow).TotalMilliseconds;
                pw.WriteShort((short)(remaining / 1000));
            }
        }

        private static void AddInventoryInfo(PacketWriter pw, MapleCharacter chr)
        {

            pw.WriteByte(chr.Inventory.EquipSlots);
            pw.WriteByte(chr.Inventory.UseSlots);
            pw.WriteByte(chr.Inventory.SetupSlots);
            pw.WriteByte(chr.Inventory.EtcSlots);
            pw.WriteByte(chr.Inventory.CashSlots);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2L));

            var inventory = chr.Inventory;

            //Equipped items
            Dictionary<short, MapleItem> equipped = inventory.GetInventory(MapleInventoryType.Equipped);
            foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -100 && kvp.Value.Position < 0))
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteShort(0);

            foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -1000 && kvp.Value.Position < -100))
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteShort(0);

            Dictionary<short, MapleItem> equipInventory = inventory.GetInventory(MapleInventoryType.Equip);
            foreach (KeyValuePair<short, MapleItem> kvp in equipInventory)
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteInt(0);

            Dictionary<short, MapleItem> useInventory = inventory.GetInventory(MapleInventoryType.Use);
            foreach (KeyValuePair<short, MapleItem> kvp in useInventory)
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteByte(0);

            Dictionary<short, MapleItem> setupInventory = inventory.GetInventory(MapleInventoryType.Setup);
            foreach (KeyValuePair<short, MapleItem> kvp in setupInventory)
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteByte(0);

            Dictionary<short, MapleItem> etcInventory = inventory.GetInventory(MapleInventoryType.Etc);
            foreach (KeyValuePair<short, MapleItem> kvp in etcInventory)
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
            pw.WriteByte(0);

            Dictionary<short, MapleItem> cashInventory = inventory.GetInventory(MapleInventoryType.Cash);
            foreach (KeyValuePair<short, MapleItem> kvp in cashInventory)
            {
                MapleItem.AddItemPosition(pw, kvp.Value);
                MapleItem.AddItemInfo(pw, kvp.Value);
            }
        }

        public static void EnterMap(MapleClient c, int mapId, byte spawnPoint, bool fromSpecialPortal = false)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.ENTER_MAP);
            pw.WriteInt(c.Channel);
            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(mapId);
            pw.WriteByte(spawnPoint);
            pw.WriteShort(c.Account.Character.HP);
            pw.WriteByte(0);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow));
            c.SendPacket(pw);
        }

        public static void SendCSInfo(MapleClient c)
        {
            MapleCharacter chr = c.Account.Character;


            var pw = new PacketWriter(SMSGHeader.ENTER_CASH_SHOP);

            MapleCharacter.AddCharInfo(pw, chr);
            pw.WriteBool(true); //IsNotBeta? lol
            pw.WriteInt(0);
            pw.WriteShort(0); //ModCashItemInfo?
            pw.WriteInt(0); //Packages? dont know, neither want to

            //Unk - long hex string

            pw.WriteZeroBytes(7);
            pw.WriteByte(0xAC);
            pw.WriteZeroBytes(6);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow));
            pw.WriteZeroBytes(7);

            c.SendPacket(pw);
        }

        public void EnableActions(bool updateClient = true)
        {
            ActionState = ActionState.ENABLED;
            if (!updateClient) return;

            var empty = new SortedDictionary<MapleCharacterStat, int>();
            UpdateStats(Client, empty, true);
        }

        public void SetActionState(ActionState state)
        {
            ActionState = state;
        }

        public bool DisableActions(ActionState newState = ActionState.DISABLED)
        {
            if (ActionState != ActionState.ENABLED)
            {
                return false;
            }
            ActionState = newState;
            return true;
        }

        public static void UpdateSingleStat(MapleClient c, MapleCharacterStat stat, int value, bool enableActions = false)
        {
            var stats = new SortedDictionary<MapleCharacterStat, int>() { { stat, value } };
            UpdateStats(c, stats, enableActions);
        }

        public static void UpdateStats(MapleClient c, SortedDictionary<MapleCharacterStat, int> stats, bool enableActions)
        {

            var pw = new PacketWriter(SMSGHeader.UPDATE_STATS);

            pw.WriteBool(enableActions);
            if (enableActions)
                c.Account.Character.ActionState = ActionState.ENABLED;
            int mask = 0;
            foreach (KeyValuePair<MapleCharacterStat, int> kvp in stats)
            {
                mask |= (int)kvp.Key;
            }
            pw.WriteInt(mask);
            foreach (KeyValuePair<MapleCharacterStat, int> kvp in stats)
            {
                switch (kvp.Key)
                {
                    case MapleCharacterStat.Skin:
                    case MapleCharacterStat.Level:
                    case MapleCharacterStat.Fatigue:
                    case MapleCharacterStat.BattleRank:
                    case MapleCharacterStat.IceGage: // not sure..
                        pw.WriteByte((byte)kvp.Value);
                        break;
                    case MapleCharacterStat.Str:
                    case MapleCharacterStat.Dex:
                    case MapleCharacterStat.Int:
                    case MapleCharacterStat.Luk:
                    case MapleCharacterStat.Ap:
                        pw.WriteShort((short)kvp.Value);
                        break;
                    case MapleCharacterStat.TraitLimit:
                        pw.WriteInt((int)kvp.Value);
                        pw.WriteInt((int)kvp.Value);
                        pw.WriteInt((int)kvp.Value);
                        break;
                    case MapleCharacterStat.Exp:
                    case MapleCharacterStat.Meso:
                        pw.WriteLong(kvp.Value);
                        break;
                    case MapleCharacterStat.Pet:
                        pw.WriteLong(kvp.Value);
                        pw.WriteLong(kvp.Value);
                        pw.WriteLong(kvp.Value);
                        break;
                    case MapleCharacterStat.Sp:
                        pw.WriteShort((short)kvp.Value);
                        break;
                    case MapleCharacterStat.Job:
                        pw.WriteShort((short)kvp.Value);
                        break;
                    default:
                        pw.WriteInt((int)kvp.Value);
                        break;
                }
            }
            c.SendPacket(pw);
        }

        public static PacketWriter RemovePlayerFromMap(int Id)
        {

            var pw = new PacketWriter(SMSGHeader.REMOVE_PLAYER);
            pw.WriteInt(Id);
            return pw;
        }

        public static PacketWriter SystemMessage(string message, short type)
        {
            var pw = new PacketWriter(SMSGHeader.SERVER_NOTICE); // SERVER_MESSAGE
            pw.WriteShort(type);
            pw.WriteMapleString(message);
            return pw;
        }

        public static PacketWriter ServerNotice(string message, byte type, int channel = 0, bool whisperIcon = false)
        {

            var pw = new PacketWriter(SMSGHeader.SERVER_NOTICE);

            pw.WriteByte(type);
            if (type == 4)
            {
                pw.WriteByte(1);
            }
            if ((type != 23) && (type != 24))
            {
                pw.WriteMapleString(message);
            }
            switch (type)
            {
                case 3:
                case 22:
                case 25:
                case 26:
                    pw.WriteByte((byte)(channel));
                    pw.WriteBool(whisperIcon);
                    break;
                case 9:
                    pw.WriteByte((byte)(channel));
                    break;
                case 12:
                    pw.WriteInt(channel);
                    break;
                case 6:
                case 11:
                case 20:
                    pw.WriteInt(0);
                    break;
                case 24:
                    pw.WriteShort(0);
                    break;
            }

            return pw;
        }

        private static void EncodeTime(PacketWriter packet, int time)
        {
            packet.WriteByte(1);
            packet.WriteInt(time);//This isn't the proper variable but it's better than Random(). It should be how long the session has been since it entered login screen
        }

        public static PacketWriter SpawnPlayer(MapleCharacter chr)
        {

            var pw = new PacketWriter(SMSGHeader.SPAWN_PLAYER);
            pw.WriteInt(chr.ID);
            pw.WriteByte(chr.Level);
            pw.WriteMapleString(chr.Name);
            pw.WriteMapleString(""); //Ultimate adventurer parent name
                                     // if (chr.Guild != null)
                                     // {
                                     //     pw.WriteMapleString(chr.Guild.Name);
                                     //     pw.WriteShort((short)chr.Guild.LogoBG);
                                     //     pw.WriteByte((byte)chr.Guild.LogoBGColor);
                                     //     pw.WriteShort((short)chr.Guild.Logo);
                                     //     pw.WriteByte((byte)chr.Guild.LogoColor);
                                     // }
                                     // else
                                     // {
            pw.WriteLong(0);
            //}

            for (int i = 0; i < 7; i++)
                pw.WriteInt(0);
            pw.WriteInt(0x00C00000);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0x00000030);
            pw.WriteInt(0);
            pw.WriteInt(0x0000003F);
            pw.WriteUInt(0x80000000);

            pw.WriteInt(-1);

            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);

            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);

            for (int i = 0; i < 10; i++)
                pw.WriteInt(0);

            int encodetime = Environment.TickCount;
            EncodeTime(pw, encodetime);
            pw.WriteInt(0);
            pw.WriteInt(0);
            for (int i = 0; i < 2; i++)
            {
                EncodeTime(pw, encodetime);
                pw.WriteShort(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
            }

            EncodeTime(pw, encodetime);
            pw.WriteInt(0);
            pw.WriteInt(0);

            EncodeTime(pw, encodetime);
            pw.WriteByte(0);

            pw.WriteUInt(0xDA77ACDE); //some constant that happens to be read by the same function as "encodetime" in ms
            pw.WriteShort(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            EncodeTime(pw, encodetime);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            EncodeTime(pw, encodetime);
            pw.WriteShort(0);
            pw.WriteShort(chr.Job);

            AddCharLook(pw, chr, true);

            for (int i = 0; i < 14; i++)
                pw.WriteInt(0);

            pw.WritePoint(chr.Position);
            pw.WriteByte(chr.Stance);
            pw.WriteShort(chr.Foothold);//$$UNSAFE$$ this should be foothold
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(1);
            pw.WriteByte(0);

            //v143: Why are these mount things still here? I thought mount levels were removed in RED
            pw.WriteInt(1); //mount level 
            pw.WriteInt(0); //mount exp
            pw.WriteInt(0); //mount fatigue
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteInt(0);
            {
                pw.WriteMapleString("Creating..."); //name of farm, creating... = not made yet
                pw.WriteInt(0); //coins
                pw.WriteInt(0); //level
                pw.WriteInt(0); //exp
                pw.WriteInt(0); //clovers
                pw.WriteInt(0); //diamonds nx currency 
                pw.WriteByte(0); //kitty power
                pw.WriteInt(0);//unk
                pw.WriteInt(0);//unk
                pw.WriteInt(1);//unk
            }
            for (int i = 0; i < 5; i++)
            {
                pw.WriteByte(0xFF);
            }

            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw;
        }
        public static PacketWriter ShowKeybindLayout(Dictionary<uint, Tuple<byte, int>> keybinds)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.KEYMAP);

            bool empty = keybinds == null || !keybinds.Any();
            pw.WriteBool(empty);
            for (byte i = 0; i < 90; i++)
            {
                Tuple<byte, int> keybind;
                if (keybinds.TryGetValue(i, out keybind))
                {
                    pw.WriteByte(keybind.Item1);
                    pw.WriteInt(keybind.Item2);
                }
                else
                {
                    pw.WriteByte(0);
                    pw.WriteInt(0);
                }
            }
            return pw;
        }

        public static PacketWriter ShowQuickSlotKeys(int[] binds)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.QUICK_SLOT);
            pw.WriteByte(1);
            if (binds.Length == 28)
            {
                for (int i = 0; i < 28; i++)
                {
                    pw.WriteInt(binds[i]);
                }
            }
            else
            {
                pw.WriteZeroBytes(112);
            }
            return pw;
        }

        #endregion

        public void Update()
        {
            if (IsBandit)
            {
                IncreaseCriticalGrowth(false);
            }
        }

        public void IncreaseCriticalGrowth(bool fromAttack)
        {
            Buff criticalGrowth = GetBuff(Bandit.CRITICAL_GROWTH);
            if (criticalGrowth != null)
            {
                if (Stats.CritRate + criticalGrowth.Stacks < 100)
                {
                    byte primeCriticalLevel = GetSkillLevel(Shadower.PRIME_CRITICAL);
                    int critInc = 2;
                    if (primeCriticalLevel > 0)
                        critInc = DataBuffer.GetCharacterSkillById(Shadower.PRIME_CRITICAL).GetEffect(primeCriticalLevel).Info[CharacterSkillStat.x];
                    criticalGrowth.Stacks += critInc;
                    if (criticalGrowth.Stacks > 100)
                        criticalGrowth.Stacks = 100;
                    Client.SendPacket(Buff.GiveBuff(criticalGrowth));
                }
                else if (fromAttack)
                {
                    criticalGrowth.Stacks = 1;
                    Client.SendPacket(Buff.GiveBuff(criticalGrowth));
                }
            }
            else
            {
                SkillEffect effect = DataBuffer.GetCharacterSkillById(Bandit.CRITICAL_GROWTH).GetEffect(1);
                criticalGrowth = new Buff(Bandit.CRITICAL_GROWTH, effect, SkillEffect.MAX_BUFF_TIME_MS, this);
                criticalGrowth.Stacks = 2;
                GiveBuff(criticalGrowth);
            }
        }

        public bool IsDead
        {
            get { lock (_hpLock) { return HP <= 0; } }
        }

        public bool IsFacingLeft => Stance % 2 != 0;

        public bool IsAdmin => Client.Account.AccountType == 3;

        #region Jobs
        public int CurrentLevelSkillBook
        {
            get
            {
                if (Job >= 2210 && Job <= 2218)
                    return Job - 2209;

                if (Level <= 30)
                    return 0;
                if (Level <= 60)
                    return 1;
                if (Level <= 100)
                    return 2;
                if (Level > 100)
                    return 3;
                return 0;
            }
        }
        public bool IsBeginnerJob => JobConstants.IsBeginnerJob(Job);
        public bool IsExplorer => Job < 600;
        public bool IsWarrior { get { return Job / 100 == 1; } }
        public bool IsFighter { get { return Job / 10 == 11; } }
        public bool IsPage { get { return Job / 10 == 12; } }
        public bool IsSpearman { get { return Job / 10 == 13; } }
        public bool IsMagician { get { return Job / 100 == 2; } }
        public bool IsFirePoisonMage { get { return Job / 10 == 21; } }
        public bool IsIceLightningMage { get { return Job / 10 == 22; } }
        public bool IsCleric { get { return Job / 10 == 23; } }
        public bool IsArcher { get { return Job / 100 == 3; } }
        public bool IsHunter { get { return Job / 10 == 31; } }
        public bool IsCrossbowman { get { return Job / 10 == 32; } }
        public bool IsThief { get { return Job / 100 == 4; } }
        public bool IsAssassin { get { return Job / 10 == 41; } }
        public bool IsBandit { get { return Job / 10 == 42; } }
        public bool IsPirate { get { return Job / 100 == 5; } }
        public bool IsBrawler { get { return Job / 10 == 51; } }
        public bool IsGunslinger { get { return Job / 10 == 52; } }
        public bool IsJett { get { return Job == 508 || Job / 10 == 57; } }
        public bool IsGameMasterJob { get { return Job / 100 == 9; } }
        public bool IsSuperGameMasterJob { get { return Job == 910; } }
        public bool IsCygnus { get { return Job / 1000 == 1; } }
        public bool IsDawnWarrior { get { return Job / 10 == 11; } }
        public bool IsBlazeWizard { get { return Job / 10 == 12; } }
        public bool IsWindArcher { get { return Job / 10 == 13; } }
        public bool IsNightWalker { get { return Job / 10 == 14; } }
        public bool IsThunderBreaker { get { return Job / 10 == 15; } }
        public bool IsHero { get { return Job / 1000 == 2; } }
        public bool IsEvan { get { return Job == 2001 || Job / 100 == 22; } }
        public bool IsMercedes { get { return Job == 2002 || Job / 100 == 23; } }
        public bool IsPhantom { get { return Job == 2003 || Job / 100 == 24; } }
        public bool IsResistance { get { return Job / 1000 == 3; } }
        public bool IsDemon { get { return Job == 3001 || Job / 100 == 31; } }
        public bool IsDemonSlayer { get { return Job == 3100 || Job / 10 == 311; } }
        public bool IsDemonAvenger { get { return Job == 3101 || Job / 10 == 312; } }
        public bool IsBattleMage { get { return Job / 100 == 32; } }
        public bool IsWildHunter { get { return Job / 100 == 33; } }
        public bool IsMechanic { get { return Job / 100 == 35; } }
        public bool IsXenon { get { return Job == 3002 || Job / 100 == 36; } }
        public bool IsSengoku { get { return Job / 1000 == 4; } }
        public bool IsHayato { get { return Job == 4001 || Job / 100 == 41; } }
        public bool IsKanna { get { return Job == 4002 || Job / 100 == 42; } }
        public bool IsMihile { get { return Job == 5000 || Job / 100 == 51; } }
        public bool IsNova { get { return Job / 1000 == 6; } }
        public bool IsKaiser { get { return Job == 6000 || Job / 100 == 61; } }
        public bool IsAngelicBuster { get { return Job == 6001 || Job / 100 == 65; } }
        public bool IsZero { get { return Job == 10000 || Job / 100 == 101; } }
        public bool IsBeastTamer { get { return Job == 11000 || Job / 100 == 112; } }
        #endregion
    }
}