using RazzleServer.Data;
using RazzleServer.DB.Models;
using RazzleServer.Packet;
using RazzleServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Player
{
    public class MapleGuild
    {
        private static Dictionary<int, MapleGuild> Guilds = new Dictionary<int, MapleGuild>();

        public int GuildID { get; set; }
        public int LeaderID = 0;
        private int _GP = 0;
        public int GP
        {
            get
            {
                lock (sync)
                {
                    return _GP;
                }
            }
            set
            {
                lock (sync)
                {
                    _GP = value;
                }
            }
        }
        public int Logo = 0;
        public short LogoColor = 0;
        public string Name;
        public string[] RankTitles = { "Master", "Jr. Master", "Member", "Member", "Member" };
        public int Capacity = 10;
        public int LogoBG = 0;
        public short LogoBGColor = 0;
        public string Notice;
        public int Signature = 0;
        public int Alliance = 0;
        public List<int> Characters = new List<int>();
        private object sync = new object();

        private MapleGuild()
        {
        }
        private static byte GuildLevel(int GP)
        {
            if (GP < 20000)
                return 1;
            else if (GP < 160000)
                return 2;
            else if (GP < 540000)
                return 3;
            else if (GP < 1280000)
                return 4;
            else if (GP < 2500000)
                return 5;
            else if (GP < 4320000)
                return 6;
            else if (GP < 6860000)
                return 7;
            else if (GP < 10240000)
                return 8;
            else
                return 9;
        }
        private static Dictionary<int, MapleGuild> LoadGuilds()
        {
            List<Guild> dbGuilds;
            using (var context = new MapleDbContext())
            {
                dbGuilds = context.Guilds.ToList();
            }
            Dictionary<int, MapleGuild> ret = new Dictionary<int, MapleGuild>();

            foreach (Guild DbGuild in dbGuilds)
            {
                MapleGuild gld = new MapleGuild();
                gld.GuildID = DbGuild.ID;
                gld.LeaderID = DbGuild.Leader;
                gld.GP = DbGuild.GP;
                gld.Logo = DbGuild.Logo;
                gld.LogoColor = DbGuild.LogoColor;
                gld.Name = DbGuild.Name;
                gld.RankTitles[0] = DbGuild.Rank1Title;
                gld.RankTitles[1] = DbGuild.Rank2Title;
                gld.RankTitles[2] = DbGuild.Rank3Title;
                gld.RankTitles[3] = DbGuild.Rank4Title;
                gld.RankTitles[4] = DbGuild.Rank5Title;
                gld.Capacity = DbGuild.Capacity;
                gld.LogoBG = DbGuild.LogoBG;
                gld.LogoBGColor = DbGuild.LogoBGColor;
                gld.Notice = DbGuild.Notice;
                gld.Signature = DbGuild.Signature;
                gld.Alliance = DbGuild.AllianceID;

                ret.Add(gld.GuildID, gld);
            }
            return ret;
        }
        public static void InitializeGuildDatabase() => Guilds = LoadGuilds();

        public static MapleGuild FindGuild(int ID) => Guilds.TryGetValue(ID, out var ret) ? ret : null;

        private void SendToAllGuildMembers(PacketWriter pw)
        {

            List<Character> DbChars;
            using (var context = new MapleDbContext())
            {
                DbChars = context.Characters.Where(x => x.GuildID == GuildID).ToList();
            }
            foreach (Character DbChar in DbChars)
            {
                MapleClient c = ServerManager.GetClientByCharacterId(DbChar.ID);
                if (c != null)
                {
                    c.SendPacket(pw);
                }
            }
        }
        public static void SaveGuildsToDatabase()
        {
            foreach (MapleGuild guild in Guilds.Values)
            {
                guild.SaveToDatabase();
            }
        }
        public bool HasCharacter(int characterId) => Characters.Contains(characterId);

        public static MapleGuild CreateGuild(string name, MapleCharacter leader)
        {
            foreach (MapleGuild g in Guilds.Values)
            {
                if (g.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return null;
                }
            }
            using (var context = new MapleDbContext())
            {
                Guild InsertGuild = new Guild()
                {
                    Leader = leader.ID,
                    Name = name
                };
                context.Guilds.Add(InsertGuild);
                Character DbChar = context.Characters.SingleOrDefault(x => x.ID == leader.ID);
                DbChar.GuildContribution = 500;
                DbChar.AllianceRank = 5;
                DbChar.GuildRank = 1;
                context.SaveChanges();

                MapleGuild gld = new MapleGuild
                {
                    GuildID = InsertGuild.ID,
                    LeaderID = leader.ID,
                    GP = 0,
                    Logo = 0,
                    LogoColor = 0,
                    Name = name,
                    Capacity = 10,
                    LogoBG = 0,
                    LogoBGColor = 0,
                    Notice = null,
                    Signature = 0,
                    Alliance = 0
                };
                Guilds.Add(gld.GuildID, gld);
                return gld;
            }
        }
        public PacketWriter GenerateKickPacket(MapleCharacter character)
        {
            var pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x35);
            pw.WriteUInt((uint)GuildID);
            pw.WriteInt(character.ID);
            pw.WriteMapleString(character.Name);
            return pw;
        }
        public PacketWriter GenerateSetMaster(MapleCharacter character)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x59);
            pw.WriteUInt((uint)GuildID);
            pw.WriteUInt((uint)LeaderID);
            pw.WriteInt(character.ID);
            pw.WriteByte(0);//unk
            return pw;
        }
        public PacketWriter GenerateChangeRankPacket(MapleCharacter character, byte newRank)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x46);
            pw.WriteUInt((uint)GuildID);
            pw.WriteInt(character.ID);
            pw.WriteByte(newRank);
            return pw;
        }
        public PacketWriter GenerateNoticeChangePacket()
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x4B);
            pw.WriteUInt((uint)GuildID);
            pw.WriteMapleString(Notice);
            return pw;
        }
        public PacketWriter GenerateGuildDisbandPacket()
        {
            var pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x38);
            pw.WriteUInt((uint)GuildID);
            return pw;
        }
        public PacketWriter GenerateGuildInvite(MapleCharacter fromcharacter)
        {
            var pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x05);
            pw.WriteUInt((uint)GuildID);
            pw.WriteMapleString(fromcharacter.Name);
            pw.WriteInt(fromcharacter.Level);
            pw.WriteInt(fromcharacter.Job);
            pw.WriteInt(0);//unknown
            return pw;
        }
        public static void UpdateCharacterGuild(MapleCharacter fromcharacter, string name)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION); // UPDATE_GUILD_NAME
            pw.WriteInt(fromcharacter.ID);
            pw.WriteMapleString(name);
            fromcharacter.Map.BroadcastPacket(pw, fromcharacter);

        }
        public void BroadcastCharacterJoinedMessage(MapleCharacter fromcharacter)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x2D);
            pw.WriteUInt((uint)GuildID);
            pw.WriteInt(fromcharacter.ID);
            pw.WriteString(fromcharacter.Name, 13);
            pw.WriteInt(fromcharacter.Job);
            pw.WriteInt(fromcharacter.Level);
            pw.WriteInt(fromcharacter.GuildRank);
            if (ServerManager.IsCharacterOnline(fromcharacter.ID))
            {
                pw.WriteInt(1);
            }
            else
            {
                pw.WriteInt(0);
            }
            pw.WriteInt(3);//nCommitment ?? "alliance rank"
            int contribution = (int)fromcharacter.GuildContribution;
            pw.WriteInt(contribution);

            SendToAllGuildMembers(pw);
        }
        public PacketWriter GenerateGuildDataPacket()
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.GUILD_OPERATION);
            pw.WriteByte(0x20);
            pw.WriteByte(1);//?
            pw.WriteUInt((uint)GuildID);
            pw.WriteMapleString(Name);
            foreach (string ranks in RankTitles)
            {
                pw.WriteMapleString(ranks);
            }
            List<Character> GuildCharacters;
            using (var context = new MapleDbContext())
            {
                GuildCharacters = context.Characters.Where(x => x.GuildID == GuildID).ToList();
            }
            pw.WriteByte((byte)GuildCharacters.Count);
            foreach (Character Character in GuildCharacters)
            {
                pw.WriteInt(Character.ID);
            }
            lock (sync)
            {
                GP = 0;
                foreach (Character Character in GuildCharacters)
                {
                    MapleClient c = ServerManager.GetClientByCharacterId(Character.ID);
                    int contribution = 0;
                    if (c == null)
                    {
                        pw.WriteString(Character.Name, 13);
                        pw.WriteInt(Character.Job);
                        pw.WriteInt(Character.Level);
                        pw.WriteInt(Character.GuildRank);

                        pw.WriteInt(0);
                        pw.WriteInt(3);//nCommitment ?? "alliance rank"
                        contribution = Character.GuildContribution;
                    }
                    else//the character might have unsaved data so we use this instead.
                    {
                        MapleCharacter ch = c.Account.Character;
                        pw.WriteString(ch.Name, 13);
                        pw.WriteInt(ch.Job);
                        pw.WriteInt(ch.Level);
                        pw.WriteInt(ch.GuildRank);
                        pw.WriteInt(1); // ch.Signature
                        pw.WriteInt(ch.AllianceRank);
                        contribution = ch.GuildContribution;
                    }
                    GP += contribution;
                    pw.WriteInt(contribution);
                }
            }
            pw.WriteInt(Capacity);
            pw.WriteShort((short)LogoBG);
            pw.WriteByte((byte)LogoBGColor);
            pw.WriteShort((short)Logo);
            pw.WriteByte((byte)LogoColor);
            pw.WriteMapleString(Notice);
            pw.WriteInt(GP);
            pw.WriteInt(Alliance);
            return pw;
        }
        public void SaveToDatabase()
        {
            using (var context = new MapleDbContext())
            {
                Guild UpdateGuild = context.Guilds.Single(x => x.ID == GuildID);
                UpdateGuild.ID = GuildID;
                UpdateGuild.Leader = LeaderID;
                UpdateGuild.GP = GP;
                UpdateGuild.Logo = Logo;
                UpdateGuild.LogoColor = LogoColor;
                UpdateGuild.LogoBG = LogoBG;
                UpdateGuild.LogoBGColor = LogoBGColor;
                UpdateGuild.Rank1Title = RankTitles[0];
                UpdateGuild.Rank2Title = RankTitles[1];
                UpdateGuild.Rank3Title = RankTitles[2];
                UpdateGuild.Rank4Title = RankTitles[3];
                UpdateGuild.Rank5Title = RankTitles[4];
                UpdateGuild.Capacity = Capacity;
                UpdateGuild.Notice = Notice;
                UpdateGuild.Signature = Signature;
                UpdateGuild.AllianceID = Alliance;
                context.SaveChanges();
            }
        }
        public void UpdateGuildData(MapleClient recipient = null)
        {
            if (recipient != null)
            {
                recipient.SendPacket(GenerateGuildDataPacket());
            }
            else
            {
                SendToAllGuildMembers(GenerateGuildDataPacket());
            }
        }
        public void ChangeNotice(string notice)
        {
            Notice = notice;
            SendToAllGuildMembers(GenerateNoticeChangePacket());
        }
        public void Disband()
        {
            List<Character> GuildCharacters;
            using (var context = new MapleDbContext())
            {
                GuildCharacters = context.Characters.Where(x => x.GuildID == GuildID).ToList();
            }
            Guilds.Remove(GuildID);

            foreach (Character character in GuildCharacters)
            {
                MapleClient c = ServerManager.GetClientByCharacterId(character.ID);
                c.Account.Character.Guild = null;
                if (c != null)
                {
                    c.SendPacket(GenerateGuildDisbandPacket());
                    UpdateCharacterGuild(c.Account.Character, "");
                    c.Account.Character.GuildContribution = 0;
                    c.Account.Character.AllianceRank = 5;
                    c.Account.Character.GuildRank = 5;
                    MapleCharacter.SaveToDatabase(c.Account.Character);
                }
            }

        }
        public void RemoveCharacter(MapleCharacter character)
        {
            character.GuildRank = 5;
            character.AllianceRank = 5;
            character.GuildContribution = 0;
            character.Guild = null;

            UpdateCharacterGuild(character, "");
            character.Client.SendPacket(GenerateGuildDisbandPacket());
            UpdateGuildData();
        }
        public void SetMaster(MapleCharacter character, MapleCharacter oldMaster)
        {
            character.GuildRank = 1;
            oldMaster.GuildRank = 2;
            SendToAllGuildMembers(GenerateSetMaster(character));
            LeaderID = character.ID;
        }
        public void ChangeRank(MapleCharacter character, byte guildrank)
        {
            if (guildrank > 1 && guildrank <= 5)
            {
                character.GuildRank = guildrank;
                SendToAllGuildMembers(GenerateChangeRankPacket(character, guildrank));
            }
        }
        public void KickCharacter(MapleCharacter character)
        {
            character.GuildRank = 5;
            character.AllianceRank = 5;
            character.GuildContribution = 0;
            character.Guild = null;

            SendToAllGuildMembers(GenerateKickPacket(character));
            UpdateCharacterGuild(character, "");
            MapleCharacter.SaveToDatabase(character);
        }
    }
}