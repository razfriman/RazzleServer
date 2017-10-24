using MapleLib.PacketLib;
using RazzleServer.Map;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Server;
using System.Collections.Generic;

namespace RazzleServer.Party
{
    public class MapleParty
    {
        private static Dictionary<int, MapleParty> Parties = new Dictionary<int, MapleParty>();
        private static int GlobalPartyID = 1;
        private static object PartyIDLock = new object();

        public int ID { get; private set; }
        public int LeaderID { get; private set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }

        private List<int> Characters = new List<int>();
        private Dictionary<int, PartyCharacterInfo> OfflineCharacterCache = new Dictionary<int, PartyCharacterInfo>();

        public MapleParty(string partyName)
        {
            Name = partyName;
            lock (PartyIDLock)
            {
                ID = GlobalPartyID++;
            }
        }

        public void Dispose()
        {
            lock (Parties)
            {
                Parties.Remove(ID);
            }
            OfflineCharacterCache = null;
        }

        public static MapleParty FindParty(int characterId)
        {
            //this method just scans each existing party for the chrId. It can become a bit clunky if we have many parties and high traffic,
            //so if we save parties to a database this method should be rewritten to search for the party id.
            lock (Parties)
            {
                foreach (MapleParty party in Parties.Values)
                {
                    if (party.Characters.Contains(characterId))
                        return party;
                }
            }
            return null;
        }

        public static MapleParty CreateParty(MapleCharacter owner, string partyName, bool privateParty)
        {
            MapleParty party = new MapleParty(partyName);
            lock (Parties)
            {
                Parties.Add(party.ID, party);
            }
            party.LeaderID = owner.ID;
            party.Characters.Add(owner.ID);
            party.IsPrivate = privateParty;
            owner.Client.Send(Packets.CreateParty(party));
            return party;
        }

        public void SetLeader(int newLeaderId)
        {
            if (Characters.Contains(newLeaderId))
            {
                LeaderID = newLeaderId;
                for (int i = 0; i < Characters.Count; i++)
                {
                    MapleClient c = ServerManager.GetClientByCharacterId(Characters[i]);
                    if (c != null)
                    {
                        c.Send(Packets.SetLeader(LeaderID, false));
                    }
                }
            }
        }

        public bool CharacterIdIsMember(int characterId) => Characters.Contains(characterId);

        public bool AddPlayer(MapleCharacter chr)
        {
            if (Characters.Count < 6 && !Characters.Contains(chr.ID))
            {
                Characters.Add(chr.ID);
                chr.Party = this;
                foreach (int i in Characters)
                {
                    MapleClient c = ServerManager.GetClientByCharacterId(i);
                    if (c != null)
                    {
                        c.Send(Packets.PlayerJoin(this, chr));
                        if (c.Account.Character.Map == chr.Map)
                        {
                            c.Send(Packets.UpdatePartyMemberHp(chr));
                            chr.Client.Send(Packets.UpdatePartyMemberHp(c.Account.Character));
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public PartyCharacterInfo GetPartyCharacterInfo(int characterId)
        {
            MapleCharacter chr = ServerManager.GetCharacterById(characterId);
            if (chr != null)
            {
                return new PartyCharacterInfo(chr.ID, chr.Name, chr.Level, chr.Job, chr.Client.Channel, chr.MapID);
            }
            else
            {
                PartyCharacterInfo chrInfo;
                lock (OfflineCharacterCache)
                {
                    if (OfflineCharacterCache.TryGetValue(characterId, out chrInfo))
                    {
                        return chrInfo;
                    }
                    else
                    {
                        //Character info is not in the cache so we have to load the chr from DB and then store it
                        chr = MapleCharacter.LoadFromDatabase(characterId, true);
                        if (chr != null)
                        {
                            chrInfo = new PartyCharacterInfo(chr.ID, chr.Name, chr.Level, chr.Job);
                            OfflineCharacterCache.Add(characterId, chrInfo);
                            return chrInfo;
                        }
                        else //There was an error loading the character from the DB
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public bool RemovePlayer(int id, bool kicked)
        {
            if (Characters.Remove(id))
            {
                MapleCharacter chr = ServerManager.GetCharacterById(id);
                if (chr != null)
                {
                    //Player is online
                    chr.Party = null;
                }
                if (Characters.Count > 0)
                {
                    if (id == LeaderID) //Player was the party leader
                    {
                        int newLeaderId = -1;
                        foreach (int i in Characters)
                        {
                            MapleCharacter newLeader = ServerManager.GetCharacterById(i); //online players first
                            if (newLeader != null)
                            {
                                newLeaderId = newLeader.ID;
                                break;
                            }
                        }
                        if (newLeaderId == -1) //No online party members 
                            newLeaderId = Characters[0]; //Then just take the first next character

                        LeaderID = newLeaderId;

                        for (int i = 0; i < Characters.Count; i++)
                        {
                            MapleClient c = ServerManager.GetClientByCharacterId(Characters[i]);
                            if (c != null)
                            {
                                c.Send(Packets.SetLeader(LeaderID, false));
                                c.Send(Packets.PlayerLeave(this, chr, c.Account.Character, false, kicked));
                            }
                        }
                        chr.Client.Send(Packets.PlayerLeave(this, chr, chr, false, kicked));
                    }
                }
                else //no players left, disband
                {
                    Dispose();
                    chr.Client.Send(Packets.PlayerLeave(this, chr, chr, true));
                }
                chr.Party = null;
                return true;
            }
            return false;
        }

        public List<MapleCharacter> GetCharactersOnMap(MapleMap map, int sourceCharacterId = 0)
        {
            List<MapleCharacter> ret = new List<MapleCharacter>();
            foreach (int i in Characters)
            {
                MapleCharacter chr;
                if (i != sourceCharacterId && (chr = map.GetCharacter(i)) != null)
                    ret.Add(chr);
            }
            return ret;
        }

        public void CacheCharacterInfo(MapleCharacter chr)
        {
            if (Characters.Contains(chr.ID))
            {
                lock (OfflineCharacterCache)
                {
                    OfflineCharacterCache.Remove(chr.ID);
                    OfflineCharacterCache.Add(chr.ID, new PartyCharacterInfo(chr.ID, chr.Name, chr.Level, chr.Job));
                }
            }
        }

        public void UpdateParty()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                MapleClient c = ServerManager.GetClientByCharacterId(Characters[i]);
                if (c != null)
                {
                    c.Send(Packets.UpdateParty(this));
                }
            }
        }

        public MapleCharacter GetLeader() => ServerManager.GetCharacterById(LeaderID);

        public void BroadcastPacket(PacketWriter packet, int chrIdFrom = 0, bool sendToSource = false)
        {
            foreach (int i in Characters)
            {
                MapleCharacter chr = ServerManager.GetCharacterById(i);
                if (chr != null && (chr.ID != chrIdFrom || sendToSource))
                {
                    chr.Client.Send(packet);
                }
            }
        }

        public static class Packets
        {
            public static PacketWriter GenerateInvite(MapleCharacter from)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x04);
                pw.WriteInt(from.ID);
                pw.WriteMapleString(from.Name);
                pw.WriteInt(from.Level);
                pw.WriteInt(from.Job);
                pw.WriteShort(0);
                return pw;
            }

            public static PacketWriter CreateParty(MapleParty party)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x10);
                pw.WriteInt(party.ID);
                pw.WriteInt(999999999); //telerock?
                pw.WriteInt(999999999); //telerock?
                pw.WriteInt(0);
                pw.WriteShort(0);
                pw.WriteShort(0);
                pw.WriteByte(0);

                pw.WriteBool(!party.IsPrivate);
                pw.WriteMapleString(party.Name);

                return pw;
            }

            public static PacketWriter UpdatePartyName(MapleParty party)
            {
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x4D);
                pw.WriteBool(!party.IsPrivate);
                pw.WriteMapleString(party.Name);
                return pw;
            }

            public static PacketWriter InviteResponse(byte response, string ign)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(response);
                pw.WriteMapleString(ign);
                return pw;
            }

            private static void AddPartyPlayersInfo(PacketWriter pw, MapleParty party)
            {
                List<PartyCharacterInfo> chrs = new List<PartyCharacterInfo>();
                foreach (int i in party.Characters)
                    chrs.Add(party.GetPartyCharacterInfo(i));

                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(i < chrs.Count ? chrs[i].ID : 0);
                }
                for (int i = 0; i < 6; i++)
                {
                    if (i < chrs.Count)
                    {
                        pw.WriteString(chrs[i].Name, 13);
                    }
                    else
                    {
                        pw.WriteZeroBytes(13);
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(i < chrs.Count ? chrs[i].Job : 0);
                }
                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(0);
                }
                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(i < chrs.Count ? chrs[i].Level : 0);
                }
                for (int i = 0; i < 6; i++)
                {
                    if (i < chrs.Count)
                    {
                        pw.WriteInt(chrs[i].Channel);
                    }
                    else
                    {
                        pw.WriteInt(-2);
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(0);
                }
                pw.WriteInt(party.LeaderID);
                for (int i = 0; i < 6; i++)
                {
                    pw.WriteInt(i < chrs.Count ? chrs[i].MapID : 0);
                }
                for (int i = 0; i < 6; i++)
                {
                    if (i < chrs.Count)
                    {
                        //todo: doors
                        //MapleCharacter chr = chrs[i];
                        pw.WriteInt(999999999);
                        pw.WriteInt(999999999);
                        pw.WriteInt(0);
                        pw.WriteInt(-1);
                        pw.WriteInt(-1);
                    }
                    else
                    {
                        pw.WriteZeroBytes(20);
                    }
                }
                pw.WriteBool(!party.IsPrivate);
                pw.WriteMapleString(party.Name);
            }

            public static PacketWriter UpdatePartyMemberHp(MapleCharacter chr)
            {
                
                var pw = new PacketWriter(ServerOperationCode.UPDATE_PARTYMEMBER_HP);
                pw.WriteInt(chr.ID);
                pw.WriteInt(chr.HP);
                pw.WriteInt(chr.MaxHP);
                return pw;
            }

            public static PacketWriter UpdateParty(MapleParty party)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x0F);
                pw.WriteInt(party.ID);
                AddPartyPlayersInfo(pw, party);
                return pw;
            }

            public static PacketWriter SetLeader(int leader, bool dc)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x30); //0x2D + 3 ? not sure todo: check this
                pw.WriteInt(leader);
                pw.WriteBool(dc);
                return pw;
            }

            public static PacketWriter PlayerJoin(MapleParty party, MapleCharacter newChar)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x18); //0x15 +3
                pw.WriteInt(party.ID);
                pw.WriteMapleString(newChar.Name);
                AddPartyPlayersInfo(pw, party);
                return pw;
            }

            public static PacketWriter PlayerLeave(MapleParty party, MapleCharacter leaveChar, MapleCharacter recipient, bool disband, bool kicked = false)
            {
                
                var pw = new PacketWriter(ServerOperationCode.PARTY_OPERATION);
                pw.WriteByte(0x15); //0x12 + 3
                pw.WriteInt(party.ID);
                pw.WriteInt(leaveChar.ID);
                pw.WriteBool(!disband);
                if (!disband)
                {
                    pw.WriteBool(kicked);
                    pw.WriteMapleString(leaveChar.Name);
                    AddPartyPlayersInfo(pw, party);
                }
                return pw;
            }
        }
    }
}
