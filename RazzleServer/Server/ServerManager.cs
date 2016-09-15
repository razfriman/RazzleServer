using RazzleServer.Map;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Server
{
    public static class ServerManager
    {
        private static Dictionary<int, MapleEvent> Events = new Dictionary<int, MapleEvent>();
        private static AutoIncrement EventId = new AutoIncrement();

        public static LoginServer LoginServer { get; set; }
        public static Dictionary<int, ChannelServer> ChannelServers { get; set; } = new Dictionary<int, ChannelServer>();

        public static ChannelServer GetChannelServer(int channel)
        {
            if (ChannelServers.ContainsKey(channel))
            {
                return ChannelServers[channel];
            }
            return null;
        }

        public static bool IsCharacterOnline(int characterId)
        {
            return GetClientByCharacterId(characterId) != null;
        }

        public static bool IsAccountOnline(int accountId)
        {
            foreach (var channel in ChannelServers.Values)
            {
                if (channel.Clients.Values.Any(client => client?.Account?.ID == accountId))
                {
                    return true;
                }
            }
            return false;
        }

        public static MapleClient GetClientByCharacterId(int chrId)
        {
            foreach (var channel in ChannelServers.Values)
            {
                var client = channel.Clients.Values.FirstOrDefault(x => x?.Account?.Character?.ID == chrId);
                if (client != null)
                {
                    return client;
                }
            }
            return null;
        }

        public static MapleClient GetClientByCharacterName(string ign)
        {
            foreach (var channel in ChannelServers.Values)
            {
                var client = channel.Clients.Values.FirstOrDefault(x => x?.Account?.Character?.Name?.ToLower() == ign);
                if (client != null)
                {
                    return client;
                }
            }
            return null;
        }

        public static MapleCharacter GetCharacterById(int chrId)
        {
            foreach (var channel in ChannelServers.Values)
            {
                var client = channel.Clients.Values.FirstOrDefault(x => x?.Account?.Character?.ID == chrId);
                if (client != null)
                {
                    return client.Account.Character;
                }
            }
            return null;
        }
        public static MapleClient GetClientByAccountId(int accountId)
        {
            foreach (var channel in ChannelServers.Values)
            {
                var client = channel.Clients.Values.FirstOrDefault(x => x?.Account?.ID == accountId);
                if (client != null)
                {
                    return client;
                }
            }
            return null;
        }

        public static MapleCharacter GetCharacterByAccountId(int accountId)
        {
            MapleClient c = GetClientByAccountId(accountId);
            return c != null ? c.Account.Character : null;
        }

        public static MapleCharacter GetCharacterByName(string ign)
        {
            foreach (var channel in ChannelServers.Values)
            {
                var client = channel.Clients.Values.FirstOrDefault(x => x?.Account?.Character?.Name?.ToLower() == ign);
                if (client != null)
                {
                    return client.Account.Character;
                }
            }
            return null;
        }

        public static Dictionary<MapleClient, bool> GetOnlineBuddies(List<int> characterIds, List<int> accountIds)
        {
            Dictionary<MapleClient, bool> onlineBuddies = new Dictionary<MapleClient, bool>();

            foreach (var channel in ChannelServers.Values)
            {
                foreach (MapleClient c in channel.Clients.Values.Where(x => x.Account?.Character != null))
                {
                    if (accountIds.Count == 0 && characterIds.Count == 0)
                        break;
                    if (!onlineBuddies.ContainsKey(c))
                    {
                        if (accountIds.Contains(c.Account.ID))
                        {
                            accountIds.Remove(c.Account.ID);
                            onlineBuddies.Add(c, true);
                        }
                        else if (characterIds.Contains(c.Account.Character.ID))
                        {
                            characterIds.Remove(c.Account.Character.ID);
                            onlineBuddies.Add(c, false);
                        }
                    }
                }
            }
            return onlineBuddies;
        }

        public static int RegisterEvent(MapleEvent Event)
        {
            int eventId = EventId.Get;
            Events.Add(eventId, Event);
            return eventId;
        }

        public static MapleEvent GetEventById(int id)
        {
            MapleEvent ret = null;
            Events.TryGetValue(id, out ret);
            return ret;
        }

        public static void UnregisterEvent(int eventId)
        {
            Events.Remove(eventId);
        }
    }
}