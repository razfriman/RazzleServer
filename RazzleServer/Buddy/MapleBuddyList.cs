using RazzleServer.DB.Models;
using RazzleServer.Packet;
using RazzleServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapleLib.PacketLib;

namespace RazzleServer.Player
{
    public class MapleBuddyList
    {
        public const string DEFAULT_GROUP = "Default Group";

        public bool Invisible { get; private set; }
        public int Capacity { get; private set; }

        private readonly Dictionary<int, MapleBuddy> CharacterBuddies;
        private readonly Dictionary<int, MapleBuddy> AccountBuddies;

        public int TotalBuddies => AccountBuddies.Count + CharacterBuddies.Count;

        public MapleBuddyList(Dictionary<int, MapleBuddy> characterBuddies, Dictionary<int, MapleBuddy> accountBuddies, int capacity)
        {
            CharacterBuddies = characterBuddies;
            AccountBuddies = accountBuddies;
            Capacity = capacity;
        }

        public void SetInvisible(bool invisible, int ownerCharacterId, int ownerAccountId, string ownerName, int currentChannel, MapleClient listOwnerClient)
        {
            if (Invisible == invisible) return;
            Invisible = invisible;
            if (invisible)
                currentChannel = -1;
            NotifyChannelChangeToBuddies(ownerCharacterId, ownerAccountId, ownerName, currentChannel);
            listOwnerClient.SendPacket(Packets.UpdateCurrentStatus(invisible));
        }

        public static MapleBuddyList LoadFromDatabase(List<Buddy> databaseBuddies, int capacity)
        {
            Dictionary<int, MapleBuddy> accountBuddies = new Dictionary<int, MapleBuddy>();
            Dictionary<int, MapleBuddy> characterBuddies = new Dictionary<int, MapleBuddy>();
            foreach (Buddy dbBuddy in databaseBuddies)
            {
                if (dbBuddy.AccountID > 0)
                {
                    if (!accountBuddies.ContainsKey(dbBuddy.BuddyAccountID))
                    {
                        accountBuddies.Add(dbBuddy.BuddyAccountID, new MapleBuddy(dbBuddy.BuddyCharacterID, dbBuddy.BuddyAccountID, dbBuddy.Name, dbBuddy.Group, dbBuddy.IsRequest, dbBuddy.Memo));
                    }
                }
                else if (!characterBuddies.ContainsKey(dbBuddy.BuddyCharacterID))
                {
                    characterBuddies.Add(dbBuddy.BuddyCharacterID, new MapleBuddy(dbBuddy.BuddyCharacterID, dbBuddy.BuddyAccountID, dbBuddy.Name, dbBuddy.Group, dbBuddy.IsRequest, dbBuddy.Memo));
                }
            }
            return new MapleBuddyList(characterBuddies, accountBuddies, capacity);
        }

        public MapleBuddy CharacterRequestAccepted(int characterId)
        {
            MapleBuddy buddy;
            if (CharacterBuddies.TryGetValue(characterId, out buddy) && buddy.IsRequest)
            {
                buddy.IsRequest = false;
                return buddy;
            }
            return null;
        }

        public MapleBuddy AccountRequestAccepted(int accountId)
        {
            MapleBuddy buddy;
            if (AccountBuddies.TryGetValue(accountId, out buddy) && buddy.IsRequest)
            {
                buddy.IsRequest = false;
                return buddy;
            }
            return null;
        }

        public bool HasCharacterBuddy(int characterId)
        {
            return CharacterBuddies.ContainsKey(characterId);
        }

        public bool HasAccountBuddy(int accountId)
        {
            return AccountBuddies.ContainsKey(accountId);
        }

        public bool MapleCharacterIsBuddy(MapleCharacter chr)
        {
            return AccountBuddies.ContainsKey(chr.AccountID) || CharacterBuddies.ContainsKey(chr.ID);
        }

        public MapleBuddy AddCharacterBuddy(int characterId, string name, string group, string memo)
        {
            if (TotalBuddies >= 50 || CharacterBuddies.ContainsKey(characterId)) return null;
            MapleBuddy buddy = new MapleBuddy(characterId, 0, name, group, false, memo);
            CharacterBuddies.Add(characterId, buddy);
            return buddy;
        }

        public MapleBuddy AddAccountBuddy(int accountId, string nickName, string group, string memo)
        {
            if (TotalBuddies >= 50 || AccountBuddies.ContainsKey(accountId)) return null;
            MapleBuddy buddy = new MapleBuddy(0, accountId, nickName, group, false, memo);
            AccountBuddies.Add(accountId, buddy);
            return buddy;
        }

        public bool RemoveCharacterBuddy(int characterId, MapleClient ownerClient)
        {
            if (!CharacterBuddies.Remove(characterId)) return false;
            ownerClient.SendPacket(Packets.RemoveBuddy(characterId, false));
            return true;
        }

        public bool RemoveAccountBuddy(int accountId, MapleClient ownerClient)
        {
            if (!AccountBuddies.Remove(accountId)) return false;
            ownerClient.SendPacket(Packets.RemoveBuddy(accountId, true));
            return true;
        }

        public MapleBuddy AddCharacterBuddyRequest(int characterId, string name)
        {
            CharacterBuddies.Remove(characterId);
            MapleBuddy newBuddy = new MapleBuddy(characterId, 0, name, DEFAULT_GROUP, true, string.Empty);
            CharacterBuddies.Add(characterId, newBuddy);
            return newBuddy;
        }

        public MapleBuddy AddAccountBuddyRequest(int accountId, string name)
        {
            AccountBuddies.Remove(accountId);
            MapleBuddy buddy = new MapleBuddy(0, accountId, name, DEFAULT_GROUP, true, string.Empty);
            AccountBuddies.Add(accountId, buddy);
            return buddy;
        }

        public bool UpdateCharacterBuddyMemo(MapleClient listOwnerClient, int buddyCharacterId, string newMemo)
        {
            MapleBuddy buddy;
            if (!CharacterBuddies.TryGetValue(buddyCharacterId, out buddy)) return false;
            buddy.Memo = newMemo;
            listOwnerClient.SendPacket(Packets.UpdateBuddy(buddy));
            return true;
        }

        public bool UpdateAccountBuddyMemo(MapleClient listOwnerClient, int buddyAccountId, string newNickName, string newMemo)
        {
            MapleBuddy buddy;
            if (!AccountBuddies.TryGetValue(buddyAccountId, out buddy)) return false;
            buddy.NickName = newNickName;
            buddy.Memo = newMemo;
            listOwnerClient.SendPacket(Packets.UpdateBuddy(buddy));
            return true;
        }

        public void NotifyChannelChangeToBuddies(int ownerCharacterId, int ownerAccountId, string ownerName, int channel, MapleClient listOwnerClient = null, bool login = false)
        {
            Dictionary<MapleClient, bool> onlineBuddies = null;
            if (!Invisible || channel == -1)
            {
                onlineBuddies = ServerManager.GetOnlineBuddies(GetCharacterBuddyIds(), GetAccountBuddyIds());
                foreach (var kvp in onlineBuddies)
                {
                    if (kvp.Value) //account buddy
                    {
                        kvp.Key.Account.Character.BuddyList.BuddyChannelChanged(kvp.Key, ownerCharacterId, ownerAccountId, ownerName, true, channel);
                    }
                    else
                    {
                        kvp.Key.Account.Character.BuddyList.BuddyChannelChanged(kvp.Key, ownerCharacterId, 0, ownerName, false, channel);
                    }
                }
            }
            if (!login) return;
            //Update buddy's status as well
            if (onlineBuddies == null)
                onlineBuddies = ServerManager.GetOnlineBuddies(GetCharacterBuddyIds(), GetAccountBuddyIds());
            foreach (var kvp in onlineBuddies.Where(kvp => !kvp.Key.Account.Character.BuddyList.Invisible))
            {
                MapleBuddy buddy;
                if (kvp.Value)
                {
                    if (AccountBuddies.TryGetValue(kvp.Key.Account.ID, out buddy))
                    {
                        buddy.CharacterID = kvp.Key.Account.Character.ID;
                        buddy.Name = kvp.Key.Account.Character.Name;
                        buddy.Channel = kvp.Key.Channel;
                        CharacterBuddies.Remove(kvp.Key.Account.Character.ID); //Remove characterbuddy if we have him as account buddy
                    }
                }
                else if (CharacterBuddies.TryGetValue(kvp.Key.Account.Character.ID, out buddy))
                {
                    buddy.CharacterID = kvp.Key.Account.Character.ID;
                    buddy.Name = kvp.Key.Account.Character.Name;
                    buddy.Channel = kvp.Key.Channel;
                }
            }
            UpdateBuddyList(listOwnerClient);
        }

        public void BuddyChannelChanged(MapleClient listOwnerClient, int characterId, int accountId, string characterName, bool accountBuddy, int channel)
        {
            bool isMyBuddy = false;
            MapleBuddy buddy;
            if (accountBuddy)
            {
                if (AccountBuddies.TryGetValue(accountId, out buddy))
                {
                    isMyBuddy = !buddy.IsRequest;
                    if (isMyBuddy)
                        CharacterBuddies.Remove(characterId);
                }
            }
            else if (CharacterBuddies.TryGetValue(characterId, out buddy))
            {
                isMyBuddy = !buddy.IsRequest;
            }
            if (!isMyBuddy) return;
            buddy.Channel = channel;
            buddy.Name = characterName;
            listOwnerClient.SendPacket(Packets.BuddyChannelUpdate(characterId, accountId, channel, Invisible, characterName));
        }

        public void UpdateBuddyList(MapleClient c)
        {
            var buddies = GetAllBuddies();
            if (buddies.Any())
                c.SendPacket(Packets.UpdateBuddyList(buddies));
        }

        public List<MapleBuddy> GetAllBuddies()
        {
            List<MapleBuddy> buddies = new List<MapleBuddy>(CharacterBuddies.Values);
            buddies.AddRange(AccountBuddies.Values);
            return buddies;
        }

        public List<int> GetCharacterBuddyIds()
        {
            return CharacterBuddies.Values.Select(buddy => buddy.CharacterID).ToList();
        }

        public List<int> GetAccountBuddyIds()
        {
            return AccountBuddies.Values.Select(buddy => buddy.AccountID).ToList();
        }

        public static class Packets
        {
            public static PacketWriter RemoveBuddy(int buddyId, bool accountBuddy)
            {
                var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.BUDDYLIST);
                pw.WriteByte(0x25);
                pw.WriteBool(accountBuddy);
                pw.WriteInt(buddyId);
                return pw;
            }

            public static void AddBuddyInfo(PacketWriter pw, MapleBuddy buddy)
            {
                pw.WriteInt(buddy.CharacterID);
                pw.WriteString(buddy.Name, 13);
                if (buddy.IsRequest)
                {
                    pw.WriteByte(buddy.AccountBuddy ? 6 : 1);
                }
                else
                {
                    pw.WriteByte(buddy.AccountBuddy ? 7 : 2);
                }
                pw.WriteInt(buddy.Channel);
                pw.WriteString(buddy.Group, 13);
                pw.WriteInt(0);
            }

            public static PacketWriter UpdateBuddy(MapleBuddy buddy)
            {
                var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.BUDDYLIST);
                // TODO: NOT UDPATED FOR V83
                pw.WriteByte(0x15);
                pw.WriteInt(buddy.CharacterID);
                pw.WriteInt(buddy.AccountID);
                AddBuddyInfo(pw, buddy);
                pw.WriteByte(0);
                return pw;
            }

            public static PacketWriter UpdateBuddyList(List<MapleBuddy> buddies)
            {
                var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.BUDDYLIST);
                pw.WriteByte(7);
                pw.WriteInt(buddies.Count);
                foreach (MapleBuddy buddy in buddies)
                {
                    AddBuddyInfo(pw, buddy);
                }
                pw.WriteZeroBytes(4 * buddies.Count); // mapID?
                return pw;
            }

            public static PacketWriter UpdateCurrentStatus(bool invisible)
            {
                var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.BUDDYLIST);
                pw.WriteByte(0x22);
                pw.WriteInt(invisible ? 1 : 0);
                return pw;
            }

            public static PacketWriter BuddyChannelUpdate(int characterId, int accountId, int channel, bool invisible, string name)
            {
                var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.BUDDYLIST);
                pw.WriteByte(0x14);
                pw.WriteInt(characterId);
                pw.WriteByte(0);
                pw.WriteInt(channel);
                return pw;
            }
        }
    }
}