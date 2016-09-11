using RazzleServer.Data;
using RazzleServer.DB.Models;
using RazzleServer.Map;
using RazzleServer.Packet;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Linq;

namespace RazzleServer.Player
{
    public class MapleCharacter : Character
    {
        public MapleClient Client { get; private set; }
        public Point Position { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public MapleMap Map { get; set; }
        public ActionState ActionState { get; set; }
        public bool Hidden { get; set; }

        private static readonly object CharacterDatabaseLock = new object();
        private readonly object HpLock = new object();

        public MapleCharacter()
        {
        }

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
            return newCharacter;
        }

        public void LoggedIn()
        {
            //     Client.SendPacket(ShowTitles());
            //     Client.SendPacket(ShowKeybindLayout(Keybinds));
            //     Client.SendPacket(ShowQuickSlotKeys(QuickSlotKeys));
            //     Client.SendPacket(SkillMacro.Packets.ShowSkillMacros(SkillMacros));

            //     Guild = MapleGuild.FindGuild(GuildId);
            //     Guild?.UpdateGuildData();

            //     Party = MapleParty.FindParty(Id);
            //     Party?.UpdateParty();

            //     BuddyList.NotifyChannelChangeToBuddies(Id, AccountId, Name, Client.Channel, Client, true);
        }

        internal void RemoveCooldown(int skillId)
        {
            throw new NotImplementedException();
        }

        public void LoggedOut()
        {
            //Guild?.UpdateGuildData();
            //Party?.CacheCharacterInfo(this);
            //Party?.UpdateParty();
            //BuddyList.NotifyChannelChangeToBuddies(ID, AccountID, Name, -1);
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
            EnterMap(Client, toMap.MapID, portal.ID, fromSpecialPortal);
            toMap.AddCharacter(this);
            this.ActionState = ActionState.ENABLED;
        }

        internal void CancelBuff(int skillId)
        {
            throw new NotImplementedException();
        }

        public void Revive(bool returnToCity = false, bool loseExp = true, bool restoreHpToFull = false)
        {
            if (loseExp)
            {
                //DoDeathExpLosePenalty();
            }

            HP = 49;
            //AddHP(restoreHpToFull ? Stats.MaxHp : 1);

            if (returnToCity)
            {
                ChangeMap(Map.ReturnMap);
            }
            else
            {
                EnableActions();
            }
            //returnToCity ? ChangeMap(Map.ReturnMap) : EnableActions();
        }

        internal void RemoveSummon(int summonSkillId)
        {
            throw new NotImplementedException();
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
            //Inventory?.Release();
            if (Map != null)
            {
                Map.RemoveCharacter(ID);
                Map = null;
            }
            // if (ChatRoom != null)
            // {
            //     ChatRoom.RemovePlayer(Id);
            //     ChatRoom = null;
            // }
            // if (!hasMigration)
            // {
            //     foreach (Buff buff in Buffs.Values.ToList())
            //         buff.CancelRemoveBuffSchedule();
            //     foreach (MapleSummon summon in Summons.Values.ToList())
            //         summon.Dispose();
            // }
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
                // foreach (var kvp in Keybinds)
                // {
                //     KeyMap insertKeyMap = new KeyMap();

                //     insertKeyMap.CharacterId = ID;
                //     insertKeyMap.Key = (byte)kvp.Key; //Posible overflow?
                //     insertKeyMap.Type = kvp.Value.Left;
                //     insertKeyMap.Action = kvp.Value.Right;

                //     context.KeyMaps.Add(insertKeyMap);
                // }
                // for (int i = 0; i < QuickSlotKeys.Length; i++)
                // {
                //     QuickSlotKeyMap dbQuickSlotKeyMap = new QuickSlotKeyMap();
                //     dbQuickSlotKeyMap.CharacterId = InsertChar.ID;
                //     dbQuickSlotKeyMap.Key = QuickSlotKeys[i];
                //     dbQuickSlotKeyMap.Index = (byte)i;
                //     context.QuickSlotKeyMaps.Add(dbQuickSlotKeyMap);
                // }
                #endregion

                #region Skills
                // foreach (Skill skill in Skills.Values)
                // {
                //     var insertSkill = new DB.Models.Skill();
                //     insertSkill.SkillId = skill.SkillID;
                //     insertSkill.CharacterId = ID;
                //     insertSkill.Level = skill.Level;
                //     insertSkill.MasterLevel = skill.MasterLevel;
                //     insertSkill.Expiration = skill.Expiration;

                //     context.Skills.Add(insertSkill);
                // }
                #endregion
                context.SaveChanges();
            }

            return ID;
        }

        public static MapleCharacter LoadFromDatabase(int characterId, bool characterScreen, MapleClient c = null)
        {
            lock (CharacterDatabaseLock)
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

                    //chr.Inventory = MapleInventory.LoadFromDatabase(chr);

                    // foreach (QuestCustomData customQuest in dbContext.QuestCustomData.Where(x => x.CharacterId == characterId))
                    // {
                    //     if (!chr.CustomQuestData.ContainsKey(customQuest.Key))
                    //         chr.CustomQuestData.Add(customQuest.Key, customQuest.Value);
                    // }

                    if (characterScreen) return chr; //No need to load more

                    #region Buddies
                    //chr.BuddyList = MapleBuddyList.LoadFromDatabase(dbContext.Buddies.Where(x => x.AccountID == chr.AccountId || x.CharacterId == characterId).ToList(), chr.BuddyCapacity);
                    #endregion

                    #region Skills
                    // var dbSkills = dbContext.Skills.Where(x => x.CharacterId == characterId);
                    // Dictionary<int, Skill> skills = new Dictionary<int, Skill>();
                    // foreach (DB.Models.Skill DbSkill in dbSkills)
                    // {
                    //     Skill skill = new Skill(DbSkill.SkillId)
                    //     {
                    //         Level = DbSkill.Level,
                    //         MasterLevel = DbSkill.MasterLevel,
                    //         Expiration = DbSkill.Expiration,
                    //         SkillExp = DbSkill.SkillExp
                    //     };
                    //     if (!skills.ContainsKey(skill.SkillId))
                    //         skills.Add(skill.SkillId, skill);
                    // }
                    // chr.SetSkills(skills);
                    #endregion

                    #region Keybinds
                    // List<KeyMap> dbKeyMaps = dbContext.KeyMaps.Where(x => x.CharacterId == characterId).ToList();
                    // Dictionary<uint, Pair<byte, int>> keyMap = new Dictionary<uint, Pair<byte, int>>();
                    // foreach (KeyMap dbKeyMap in dbKeyMaps)
                    // {
                    //     if (!keyMap.ContainsKey(dbKeyMap.Key))
                    //     {
                    //         keyMap.Add(dbKeyMap.Key, new Pair<byte, int>(dbKeyMap.Type, dbKeyMap.Action));
                    //     }
                    // }
                    // chr.Keybinds = keyMap;

                    // List<QuickSlotKeyMap> dbQuickSlotKeyMaps = dbContext.QuickSlotKeyMaps.Where(x => x.CharacterId == characterId).ToList();
                    // int[] quickSlots = new int[28];
                    // foreach (QuickSlotKeyMap dbQuickSlotKeyMap in dbQuickSlotKeyMaps)
                    // {
                    //     quickSlots[dbQuickSlotKeyMap.Index] = dbQuickSlotKeyMap.Key;
                    // }
                    // chr.QuickSlotKeys = quickSlots;

                    // List<DbSkillMacro> dbSkillMacros = dbContext.SkillMacros.Where(x => x.CharacterId == characterId).ToList();
                    // foreach (DbSkillMacro dbSkillMacro in dbSkillMacros)
                    // {
                    //     chr.SkillMacros[dbSkillMacro.Index] = new SkillMacro(dbSkillMacro.Name, dbSkillMacro.ShoutName, dbSkillMacro.Skill1, dbSkillMacro.Skill2, dbSkillMacro.Skill3);
                    // }

                    #endregion

                    #region Cooldowns
                    // List<SkillCooldown> DbSkillCooldowns = dbContext.SkillCooldowns.Where(x => x.CharacterId == characterId).ToList();
                    // foreach (SkillCooldown DbSkillCooldown in DbSkillCooldowns)
                    // {
                    //     if (!chr.Cooldowns.ContainsKey(DbSkillCooldown.SkillId))
                    //     {
                    //         DateTime startTime = new DateTime(DbSkillCooldown.StartTime);
                    //         uint duration = (uint)DbSkillCooldown.Length;
                    //         uint remaining = (uint)(startTime.AddMilliseconds(duration) - DateTime.UtcNow).TotalMilliseconds;
                    //         if (remaining <= 2000) // less than 2 seconds is not worth the effort
                    //             continue;
                    //         chr.AddCooldownSilent(DbSkillCooldown.SkillId, duration, startTime);
                    //     }
                    // }
                    #endregion

                    #region Quests
                    // List<QuestStatus> DbQuestStatuses = dbContext.QuestStatus.Where(x => x.CharacterId == characterId).ToList();
                    // foreach (QuestStatus DbQuestStatus in DbQuestStatuses)
                    // {
                    //     MapleQuestStatus status = (MapleQuestStatus)DbQuestStatus.Status;
                    //     int questId = DbQuestStatus.Quest;
                    //     if (status == MapleQuestStatus.InProgress)
                    //     {
                    //         WzQuest info = DataBuffer.GetQuestById((ushort)questId);
                    //         if (info != null)
                    //         {
                    //             string data = DbQuestStatus.CustomData ?? "";
                    //             MapleQuest quest = null;
                    //             if (info.FinishRequirements.Where(x => x.Type == QuestRequirementType.mob).Any())
                    //             {
                    //                 List<QuestMobStatus> DbQuestStatusesMobs = dbContext.QuestStatusMobs.Where(x => x.QuestStatusId == DbQuestStatus.Id).ToList();
                    //                 Dictionary<int, int> mobs = new Dictionary<int, int>();
                    //                 foreach (QuestMobStatus DbQuestStatusMobs in DbQuestStatusesMobs)
                    //                 {
                    //                     int mobId = DbQuestStatusMobs.Mob;
                    //                     if (mobId > 0)
                    //                         mobs.Add(mobId, DbQuestStatusMobs.Count);
                    //                 }
                    //                 quest = new MapleQuest(info, status, data, mobs);
                    //             }
                    //             else
                    //             {
                    //                 quest = new MapleQuest(info, status, data);
                    //             }
                    //             chr.StartedQuests.Add(questId, quest);
                    //         }
                    //     }
                    //     else if (status == MapleQuestStatus.Completed)
                    //     {
                    //         if (!chr.CompletedQuests.ContainsKey(questId))
                    //             chr.CompletedQuests.Add(questId, 0x4E35FF7B); //TODO: real date
                    //     }
                    // }
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
            lock (CharacterDatabaseLock)
            {
                using (MapleDbContext dbContext = new MapleDbContext())
                {
                    // chr.GuildId = chr.Guild == null ? 0 : chr.Guild.GuildId;

                    // if (chr.Map != null)
                    // {
                    //     int forcedReturn = DataBuffer.GetMapById(chr.Map.MapId).ForcedReturn;
                    //     if (forcedReturn != 999999999)
                    //     {
                    //         chr.MapId = forcedReturn;
                    //         chr.SpawnPoint = 0;
                    //     }
                    //     else
                    //     {
                    //         chr.MapId = chr.Map.MapId;
                    //         chr.SpawnPoint = chr.Map.GetClosestSpawnPointId(chr.Position);
                    //     }
                    // }
                    // else
                    // {
                    //     chr.SpawnPoint = 0;
                    // }

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

                    //chr.Inventory.SaveToDatabase();

                    #region Skills
                    // List<DB.Models.Skill> DbSkills = dbContext.Skills.Where(x => x.CharacterId == chr.Id).ToList();
                    // foreach (DB.Models.Skill DbSkill in DbSkills)
                    // {
                    //     if (!chr.Skills.ContainsKey(DbSkill.SkillId)) //skill was removed                                 
                    //         dbContext.Skills.Remove(DbSkill);
                    // }
                    // foreach (Skill skill in chr.Skills.Values)
                    // {
                    //     DB.Models.Skill dbSkill = DbSkills.Where(x => x.SkillId == skill.SkillId).FirstOrDefault();
                    //     if (dbSkill != null) //Update                   
                    //     {
                    //         dbSkill.Level = skill.Level;
                    //         dbSkill.MasterLevel = skill.MasterLevel;
                    //         dbSkill.Expiration = skill.Expiration;
                    //         dbSkill.SkillExp = skill.SkillExp;
                    //     }
                    //     else //Insert
                    //     {
                    //         DB.Models.Skill InsertSkill = new DB.Models.Skill();
                    //         InsertSkill.CharacterId = chr.Id;
                    //         InsertSkill.SkillId = skill.SkillId;
                    //         InsertSkill.Level = skill.Level;
                    //         InsertSkill.MasterLevel = skill.MasterLevel;
                    //         InsertSkill.Expiration = skill.Expiration;
                    //         InsertSkill.SkillExp = skill.SkillExp;
                    //         dbContext.Skills.Add(InsertSkill);
                    //     }
                    // }
                    #endregion

                    #region Keybinds
                    // if (chr.KeybindsChanged)
                    // {
                    //     dbContext.KeyMaps.RemoveRange(dbContext.KeyMaps.Where(x => x.CharacterId == chr.Id));
                    //     foreach (var kvp in chr.Keybinds)
                    //     {
                    //         KeyMap insertKeyMap = new KeyMap();
                    //         insertKeyMap.CharacterId = chr.Id;
                    //         insertKeyMap.Key = (byte)kvp.Key; //Posible overflow?
                    //         insertKeyMap.Type = kvp.Value.Left;
                    //         insertKeyMap.Action = kvp.Value.Right;
                    //         dbContext.KeyMaps.Add(insertKeyMap);
                    //     }
                    // }
                    // if (chr.QuickSlotKeyBindsChanged)
                    // {
                    //     dbContext.QuickSlotKeyMaps.RemoveRange(dbContext.QuickSlotKeyMaps.Where(x => x.CharacterId == chr.Id));
                    //     for (int i = 0; i < chr.QuickSlotKeys.Length; i++)
                    //     {
                    //         int key = chr.QuickSlotKeys[i];
                    //         if (key > 0)
                    //         {
                    //             QuickSlotKeyMap dbQuickSlotKeyMap = new QuickSlotKeyMap();
                    //             dbQuickSlotKeyMap.CharacterId = chr.Id;
                    //             dbQuickSlotKeyMap.Key = key;
                    //             dbQuickSlotKeyMap.Index = (byte)i;
                    //             dbContext.QuickSlotKeyMaps.Add(dbQuickSlotKeyMap);
                    //         }
                    //     }
                    // }
                    // List<DbSkillMacro> dbSkillMacros = dbContext.SkillMacros.Where(x => x.CharacterId == chr.Id).ToList();
                    // foreach (DbSkillMacro dbSkillMacro in dbSkillMacros)
                    // {
                    //     if (chr.SkillMacros[dbSkillMacro.Index] == null)
                    //         dbContext.SkillMacros.Remove(dbSkillMacro);
                    // }
                    // for (int i = 0; i < 5; i++)
                    // {
                    //     if (chr.SkillMacros[i] != null && chr.SkillMacros[i].Changed)
                    //     {
                    //         SkillMacro macro = chr.SkillMacros[i];
                    //         DbSkillMacro dbSkillMacro = dbSkillMacros.FirstOrDefault(x => x.Index == i);
                    //         if (dbSkillMacro != null)
                    //         {
                    //             dbSkillMacro.Name = macro.Name;
                    //             dbSkillMacro.ShoutName = macro.ShoutName;
                    //             dbSkillMacro.Skill1 = macro.Skills[0];
                    //             dbSkillMacro.Skill2 = macro.Skills[1];
                    //             dbSkillMacro.Skill3 = macro.Skills[2];
                    //         }
                    //         else
                    //         {
                    //             dbSkillMacro = new DbSkillMacro { Index = (byte)i, CharacterId = chr.Id, Name = macro.Name, ShoutName = macro.ShoutName, Skill1 = macro.Skills[0], Skill2 = macro.Skills[1], Skill3 = macro.Skills[2] };
                    //             dbContext.SkillMacros.Add(dbSkillMacro);
                    //         }
                    //         macro.Changed = false;
                    //     }
                    // }
                    #endregion

                    #region Buddies
                    // List<MapleBuddy> buddies = chr.BuddyList.GetAllBuddies();
                    // var currentDbBuddies = dbContext.Buddies.Where(x => x.AccountId == chr.AccountId || x.CharacterId == chr.Id);
                    // //Removed buddies:
                    // foreach (Buddy b in currentDbBuddies)
                    // {
                    //     bool accbuddy = b.BuddyAccountId > 0;
                    //     if (accbuddy)
                    //     {
                    //         if (!buddies.Exists(x => x.AccountId == b.BuddyAccountId)) //check if the character's buddlist contains the buddy that is in the database
                    //             dbContext.Buddies.Remove(b);
                    //     }
                    //     else
                    //     {
                    //         if (!buddies.Exists(x => x.CharacterId == b.BuddyCharacterId)) //ditto for non-account buddy
                    //             dbContext.Buddies.Remove(b);
                    //     }
                    // }

                    // foreach (MapleBuddy buddy in buddies)
                    // {
                    //     Buddy dbBuddy;
                    //     if ((dbBuddy = currentDbBuddies.FirstOrDefault(x => x.BuddyAccountId == buddy.AccountId || x.BuddyCharacterId == buddy.CharacterId)) != null)
                    //     {
                    //         dbBuddy.Name = buddy.NickName;
                    //         dbBuddy.Group = buddy.Group;
                    //         dbBuddy.Memo = buddy.Memo;
                    //         dbBuddy.IsRequest = buddy.IsRequest;
                    //     }
                    //     else
                    //     {
                    //         Buddy newBuddy;
                    //         if (buddy.AccountBuddy)
                    //         {
                    //             newBuddy = new Buddy()
                    //             {
                    //                 AccountId = chr.AccountId,
                    //                 BuddyAccountId = buddy.AccountId,
                    //                 Name = buddy.NickName,
                    //                 Group = buddy.Group,
                    //                 Memo = buddy.Memo,
                    //                 IsRequest = buddy.IsRequest
                    //             };
                    //         }
                    //         else
                    //         {
                    //             newBuddy = new Buddy()
                    //             {
                    //                 CharacterId = chr.Id,
                    //                 BuddyCharacterId = buddy.CharacterId,
                    //                 Name = buddy.NickName,
                    //                 Group = buddy.Group,
                    //                 Memo = buddy.Memo,
                    //                 IsRequest = buddy.IsRequest
                    //             };
                    //         }
                    //         dbContext.Buddies.Add(newBuddy);
                    //     }
                    // }
                    #endregion

                    #region Cooldowns
                    // dbContext.SkillCooldowns.RemoveRange(dbContext.SkillCooldowns.Where(x => x.CharacterId == chr.Id));
                    // foreach (var kvp in chr.Cooldowns)
                    // {
                    //     if (DateTime.UtcNow <= (kvp.Value.StartTime.AddMilliseconds(kvp.Value.Duration)))
                    //     {
                    //         SkillCooldown InserSkillCooldown = new SkillCooldown();
                    //         InserSkillCooldown.CharacterId = chr.Id;
                    //         InserSkillCooldown.SkillId = kvp.Key;
                    //         if (kvp.Value.Duration > int.MaxValue)
                    //             InserSkillCooldown.Length = int.MaxValue;
                    //         else
                    //             InserSkillCooldown.Length = (int)kvp.Value.Duration;
                    //         InserSkillCooldown.StartTime = kvp.Value.StartTime.Ticks;
                    //         dbContext.SkillCooldowns.Add(InserSkillCooldown);
                    //     }
                    // }

                    #endregion

                    #region Quests
                    // var dbCustomQuestData = dbContext.QuestCustomData.Where(x => x.CharacterId == chr.Id).ToList();
                    // foreach (var dbCustomQuest in dbCustomQuestData)
                    // {
                    //     if (chr.CustomQuestData.All(x => x.Key != dbCustomQuest.Key)) //doesn't exist in current chr.CustomQuestData but it does in the DB
                    //     {
                    //         dbContext.QuestCustomData.Remove(dbCustomQuest); //Delete it from the DB
                    //     }
                    // }
                    // foreach (var kvp in chr.CustomQuestData)
                    // {
                    //     QuestCustomData dbCustomQuest = dbCustomQuestData.FirstOrDefault(x => x.Key == kvp.Key);
                    //     if (dbCustomQuest != null)
                    //     {
                    //         dbCustomQuest.Value = kvp.Value;
                    //     }
                    //     else
                    //     {
                    //         QuestCustomData newCustomQuest = new QuestCustomData { CharacterId = chr.Id, Key = kvp.Key, Value = kvp.Value };
                    //         dbContext.QuestCustomData.Add(newCustomQuest);
                    //     }
                    // }
                    // List<QuestStatus> databaseQuests = dbContext.QuestStatus.Where(x => x.CharacterId == chr.Id).ToList();
                    // List<QuestStatus> startedDatabaseQuests = databaseQuests.Where(x => x.Status == 1).ToList();
                    // List<QuestStatus> completedDatabaseQuests = databaseQuests.Where(x => x.Status == 2).ToList();
                    // foreach (QuestStatus qs in startedDatabaseQuests)
                    // {
                    //     if (!chr.StartedQuests.ContainsKey(qs.Quest)) //quest in progress was removed or forfeited
                    //     {
                    //         dbContext.QuestStatusMobs.RemoveRange(dbContext.QuestStatusMobs.Where(x => x.QuestStatusId == qs.Id));
                    //         dbContext.QuestStatus.Remove(qs);
                    //     }
                    // }
                    // foreach (var questPair in chr.StartedQuests)
                    // {
                    //     MapleQuest quest = questPair.Value;
                    //     QuestStatus dbQuestStatus = startedDatabaseQuests.FirstOrDefault(x => x.Quest == questPair.Key);
                    //     if (dbQuestStatus != null) //record exists
                    //     {
                    //         dbQuestStatus.CustomData = quest.Data;
                    //         dbQuestStatus.Status = (byte)quest.State;
                    //         if (quest.HasMonsterKillObjectives)
                    //         {
                    //             List<QuestMobStatus> qmsList = dbContext.QuestStatusMobs.Where(x => x.QuestStatusId == dbQuestStatus.Id).ToList();
                    //             foreach (var mobPair in quest.MonsterKills)
                    //             {
                    //                 QuestMobStatus qms = qmsList.FirstOrDefault(x => x.Mob == mobPair.Key);
                    //                 if (qms != null) //record exists                                
                    //                     qms.Count = mobPair.Value;
                    //                 else //doesnt exist yet, need to insert
                    //                 {
                    //                     qms = new QuestMobStatus();
                    //                     qms.Mob = mobPair.Key;
                    //                     qms.Count = mobPair.Value;
                    //                     qms.QuestStatusId = dbQuestStatus.Id;
                    //                     dbContext.QuestStatusMobs.Add(qms);
                    //                 }
                    //             }
                    //         }
                    //     }
                    //     else //doesnt exist yet, need to insert
                    //     {
                    //         dbQuestStatus = new QuestStatus();
                    //         dbQuestStatus.CharacterId = chr.Id;
                    //         dbQuestStatus.Quest = questPair.Key;
                    //         dbQuestStatus.Status = (byte)quest.State;
                    //         dbQuestStatus.CustomData = quest.Data;
                    //         dbContext.QuestStatus.Add(dbQuestStatus);
                    //         if (quest.HasMonsterKillObjectives)
                    //         {
                    //             dbContext.SaveChanges();
                    //             foreach (var kvp in quest.MonsterKills)
                    //             {
                    //                 QuestMobStatus qms = new QuestMobStatus();
                    //                 qms.QuestStatusId = dbQuestStatus.Id;
                    //                 qms.Mob = kvp.Key;
                    //                 qms.Count = kvp.Value;
                    //                 dbContext.QuestStatusMobs.Add(qms);
                    //             }
                    //         }
                    //     }
                    // }
                    // foreach (var questPair in chr.CompletedQuests)
                    // {
                    //     if (!completedDatabaseQuests.Where(x => x.Quest == questPair.Key).Any()) //completed quest isn't in the completed Database yet
                    //     {
                    //         QuestStatus qs = databaseQuests.Where(x => x.Quest == questPair.Key).FirstOrDefault();
                    //         if (qs != null) //quest is in StartedQuests database
                    //         {
                    //             dbContext.QuestStatusMobs.RemoveRange(dbContext.QuestStatusMobs.Where(x => x.QuestStatusId == qs.Id));
                    //             qs.Status = (byte)MapleQuestStatus.Completed;
                    //             qs.CompleteTime = questPair.Value;
                    //         }
                    //         else //not in database yet
                    //         {
                    //             qs = new QuestStatus();
                    //             qs.CharacterId = chr.Id;
                    //             qs.Quest = questPair.Key;
                    //             qs.CompleteTime = questPair.Value;
                    //             qs.Status = (byte)MapleQuestStatus.Completed;
                    //             dbContext.QuestStatus.Add(qs);
                    //         }
                    //     }
                    // }
                    #endregion

                    dbContext.SaveChanges();
                }
            }
        }

        public void Bind(MapleClient c)
        {
            Client = c;
            //Inventory.Bind(this);
            ActionState = ActionState.ENABLED;
        }

        #region Doors
        // public bool HasDoor(int skillId)
        // {
        //     lock (Doors)
        //     {
        //         List<SpecialPortal> sameSkillPortals = Doors.Where(x => x.SkillId == skillId).ToList();
        //         int count = sameSkillPortals.Count;
        //         foreach (SpecialPortal portal in sameSkillPortals)
        //         {
        //             if (DateTime.UtcNow >= portal.Expiration)
        //             {
        //                 portal.FromMap.RemoveStaticObject(portal.ObjectId, false);
        //                 Doors.Remove(portal);
        //                 count--;
        //             }
        //         }
        //         return count > 0;
        //     }
        // }

        // public void CancelDoor(int skillId = 0)
        // {
        //     List<SpecialPortal> sameSkillDoors;
        //     lock (Doors)
        //     {
        //         sameSkillDoors = Doors.Where(x => skillId == 0 ? true : x.SkillId == skillId).ToList();
        //     }
        //     foreach (SpecialPortal door in sameSkillDoors)
        //     {
        //         door.FromMap.RemoveStaticObject(door.ObjectId, false);
        //     }
        // }

        // public void RemoveDoor(int skillId)
        // {
        //     lock (Doors)
        //     {
        //         Doors.RemoveAll(x => x.SkillId == skillId);
        //     }
        // }

        // public void AddDoor(SpecialPortal door)
        // {
        //     lock (Doors)
        //     {
        //         Doors.Add(door);
        //     }
        // }
        #endregion

        #region Packets
        public static PacketWriter ShowExpFromMonster(int exp)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteByte(3);
            pw.WriteByte(1);
            pw.WriteInt(exp);
            pw.WriteZeroBytes(9);
            return pw;
        }

        public static PacketWriter ShowGainMapleCharacterStat(int amount, MapleCharacterStat stat)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteByte(0x11);
            pw.WriteLong(stat.Value);
            pw.WriteInt(amount);
            return pw;
        }


        public static void AddCharEntry(PacketWriter pw, MapleCharacter chr)
        {
            AddCharStats(pw, chr);
            AddCharLook(pw, chr, true);

            pw.WriteShort(0);
            // If world rank enabled
            //pw.WriteInt(0); // Rank
            //pw.WriteInt(0); // Rank Move
            //pw.WriteInt(0); // Job Rank
            //pw.WriteInt(0); // Job Rank Move
        }

        public static void AddCharStats(PacketWriter pw, MapleCharacter chr, bool CashShop = false)
        {
            pw.WriteInt(chr.ID);
            pw.WriteStaticString(chr.Name.PadRight(13, '\0'));
            pw.WriteByte(chr.Gender);
            pw.WriteByte(chr.Skin);
            pw.WriteInt(chr.Face);
            pw.WriteInt(chr.Hair);

            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

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

            pw.WriteInt(0);
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

            // Visible Items - repeat
            //byte - key
            //int value
            pw.WriteByte(0xFF);

            // Masked Items - repeat
            //byte - key
            //int value
            pw.WriteByte(0xFF);


            pw.WriteInt(0); // Weapon
            pw.WriteInt(0); // Shield
            pw.WriteLong(0);
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

            pw.WriteUInt((uint)chr.Mesos);
            AddInventoryInfo(pw, chr);
            AddSkillInfo(pw, chr);
            AddQuestInfo(pw, chr);
            pw.WriteLong(0); // rings
            for (int x = 0; x < 15; x++)
            {
                // CHAR INFO MAGIC
                pw.WriteBytes(new byte[] { 0xFF, 0xC9, 0x9A, 0x3B });
            }
            AddMonsterBookInfo(pw, chr);
            pw.WriteShort(0);
            pw.WriteShort(0);
            pw.WriteShort(0);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow)); //current time
            c.SendPacket(pw);
        }

        private static void AddQuestInfo(PacketWriter pw, MapleCharacter chr)
        {
            pw.WriteShort(0); // started.size
            //for (MapleQuestStatus q : started)
            //{
            //    mplew.writeShort(q.getQuest().getId());
            //    mplew.writeMapleAsciiString(q.getQuestData());
            //}
            pw.WriteShort(0);// completed.size
            //for (MapleQuestStatus q : completed)
            //{
            //    mplew.writeShort(q.getQuest().getId());
            //    mplew.writeLong(q.getCompletionTime());
            //}
        }

        private static void AddSkillInfo(PacketWriter pw, MapleCharacter chr)
        {

            pw.WriteByte(0);
            pw.WriteShort(0); // skills.size
            //for (Entry<ISkill, MapleCharacter.SkillEntry> skill : skills.entrySet())
            //{
            //    mplew.writeInt(skill.getKey().getId());
            //    mplew.writeInt(skill.getValue().skillevel);

            //    mplew.write(0);
            //    mplew.write(ITEM_MAGIC);
            //    mplew.writeInt(400967355);
            //    mplew.write(2);

            //    if (skill.getKey().isFourthJob())
            //    {
            //        mplew.writeInt(skill.getValue().masterlevel);
            //    }
            //}
            pw.WriteShort(0); // cooldowns.size
            //for (PlayerCoolDownValueHolder cooling : chr.getAllCooldowns())
            //{
            //    mplew.writeInt(cooling.skillId);
            //    int timeLeft = (int)(cooling.length + cooling.startTime - System.currentTimeMillis());
            //    mplew.writeShort(timeLeft / 1000);
            //}
        }

        private static void AddInventoryInfo(PacketWriter pw, MapleCharacter chr)
        {
            for (byte i = 1; i <= 5; i++)
            {
                pw.WriteByte(0);
                //mplew.write(chr.getInventory(MapleInventoryType.getByType(i)).getSlotLimit());
            }
            pw.WriteHexString("00 40 E0 FD 3B 37 4F 01");

            //MapleInventory iv = chr.getInventory(MapleInventoryType.EQUIPPED);
            //Collection<IItem> equippedC = iv.list();
            //List<Item> equipped = new ArrayList<Item>(equippedC.size());
            //List<Item> equippedCash = new ArrayList<Item>(equippedC.size());
            //for (IItem item : equippedC)
            //{
            //    if (item.getPosition() <= -100)
            //    {
            //        equippedCash.add((Item)item);
            //    }
            //    else
            //    {
            //        equipped.add((Item)item);
            //    }
            //}
            //Collections.sort(equipped);
            //for (Item item : equipped)
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteShort(0);
            //for (Item item : equippedCash)
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteShort(0);
            //for (IItem item : chr.getInventory(MapleInventoryType.EQUIP).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteInt(0);
            //for (IItem item : chr.getInventory(MapleInventoryType.USE).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteByte(0);
            //for (IItem item : chr.getInventory(MapleInventoryType.SETUP).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteByte(0);
            //for (IItem item : chr.getInventory(MapleInventoryType.ETC).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            pw.WriteByte(0);
            //for (IItem item : chr.getInventory(MapleInventoryType.CASH).list())
            //{
            //    addItemInfo(mplew, item);
            //}
        }

        public static void EnterMap(MapleClient c, int mapId, byte spawnPoint, bool fromSpecialPortal = false)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.ENTER_MAP);
            pw.WriteShort(2);
            pw.WriteLong(1);
            pw.WriteLong(2);
            pw.WriteInt(c.Channel);
            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteByte(fromSpecialPortal ? (byte)2 : (byte)3);
            pw.WriteLong(0);
            pw.WriteInt(mapId);
            pw.WriteByte(spawnPoint);
            pw.WriteInt(c.Account.Character.HP);
            pw.WriteByte(0);
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow));
            pw.WriteInt(100);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(1);
            pw.WriteZeroBytes(6);

            c.SendPacket(pw);
        }

        public static void SendCSInfo(MapleClient c)
        {
            MapleCharacter chr = c.Account.Character;
            PacketWriter pw = new PacketWriter();

            pw.WriteHeader(SMSGHeader.ENTER_CASH_SHOP);

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
            //ActionState = ActionState.Enabled;
            if (!updateClient) return;
            //SortedDictionary<MapleCharacterStat, long> empty = new SortedDictionary<MapleCharacterStat, long>();
            //UpdateStats(Client, empty, true);
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

        public static void UpdateSingleStat(MapleClient c, MapleCharacterStat stat, long value, bool enableActions = false)
        {
            //SortedDictionary<MapleCharacterStat, long> stats = new SortedDictionary<MapleCharacterStat, long>() { { stat, value } };
            //UpdateStats(c, stats, enableActions);
        }

        // public static void UpdateStats(MapleClient c, SortedDictionary<MapleCharacterStat, long> stats, bool enableActions)
        // {
        //     PacketWriter pw = new PacketWriter();
        //     pw.WriteHeader(SMSGHeader.UpdateStats);

        //     pw.WriteBool(enableActions);
        //     if (enableActions)
        //         c.Account.Character.ActionState = ActionState.Enabled;
        //     long mask = 0;
        //     foreach (KeyValuePair<MapleCharacterStat, long> kvp in stats)
        //     {
        //         mask |= (long)kvp.Key;
        //     }
        //     pw.WriteLong(mask);
        //     foreach (KeyValuePair<MapleCharacterStat, long> kvp in stats)
        //     {
        //         switch (kvp.Key)
        //         {
        //             case MapleCharacterStat.Skin:
        //             case MapleCharacterStat.Level:
        //             case MapleCharacterStat.Fatigue:
        //             case MapleCharacterStat.BattleRank:
        //             case MapleCharacterStat.IceGage: // not sure..
        //                 pw.WriteByte((byte)kvp.Value);
        //                 break;
        //             case MapleCharacterStat.Str:
        //             case MapleCharacterStat.Dex:
        //             case MapleCharacterStat.Int:
        //             case MapleCharacterStat.Luk:
        //             case MapleCharacterStat.Ap:
        //                 pw.WriteShort((short)kvp.Value);
        //                 break;
        //             case MapleCharacterStat.TraitLimit:
        //                 pw.WriteInt((int)kvp.Value);
        //                 pw.WriteInt((int)kvp.Value);
        //                 pw.WriteInt((int)kvp.Value);
        //                 break;
        //             case MapleCharacterStat.Exp:
        //             case MapleCharacterStat.Meso:
        //                 pw.WriteLong(kvp.Value);
        //                 break;
        //             case MapleCharacterStat.Pet:
        //                 pw.WriteLong(kvp.Value);
        //                 pw.WriteLong(kvp.Value);
        //                 pw.WriteLong(kvp.Value);
        //                 break;
        //             case MapleCharacterStat.Sp:
        //                 if (c.Account.Character.IsSeparatedSpJob)
        //                     AddSeparatedSP(c.Account.Character, pw);
        //                 else
        //                     pw.WriteShort((short)kvp.Value);
        //                 break;
        //             case MapleCharacterStat.Job:
        //                 pw.WriteShort((short)kvp.Value);
        //                 pw.WriteShort(c.Account.Character.SubJob); //new v144
        //                 break;
        //             default:
        //                 pw.WriteInt((int)kvp.Value);
        //                 break;
        //         }
        //     }
        //     pw.WriteByte(0xFF);
        //     if (mask == 0 && !enableActions)
        //     {
        //         pw.WriteByte(0);
        //     }
        //     pw.WriteInt(0);

        //     c.SendPacket(pw);
        // }

        public static PacketWriter RemovePlayerFromMap(int Id)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.REMOVE_PLAYER);
            pw.WriteInt(Id);
            return pw;
        }

        public static PacketWriter SystemMessage(string message, short type)
        {
            PacketWriter pw = new PacketWriter();
            //pw.WriteHeader(SMSGHeader.SYSTEM_MESSAGE);
            pw.WriteShort(type);
            pw.WriteMapleString(message);
            return pw;
        }

        public static PacketWriter ServerNotice(string message, byte type, int channel = 0, bool whisperIcon = false)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SERVER_NOTICE);

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
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SPAWN_PLAYER);
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

            int encodetime = System.Environment.TickCount;
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

        public bool IsDead
        {
            get { lock (HpLock) { return HP <= 0; } }
        }

        public bool IsFacingLeft => Stance % 2 != 0;

        public bool IsStaff => Client.Account.AccountType >= 2;

        public bool IsAdmin => Client.Account.AccountType == 3;

        #endregion
    }
}