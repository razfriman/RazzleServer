using RazzleServer.Map;
using RazzleServer.Player;
using RazzleServer.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.IO;

namespace RazzleServer.Server
{
    public static class ServerManager
    {
        private static readonly ILogger Log = LogManager.Log;

        private static Dictionary<int, MapleEvent> Events = new Dictionary<int, MapleEvent>();

        private static AutoIncrement EventId = new AutoIncrement();

        public static LoginServer LoginServer { get; set; }

        public static Dictionary<int, ChannelServer> ChannelServers { get; set; } = new Dictionary<int, ChannelServer>();

        public static ChannelServer GetChannelServer(int channel) => ChannelServers.ContainsKey(channel) ? ChannelServers[channel]  : null;

        public static bool IsCharacterOnline(int characterId) => GetClientByCharacterId(characterId) != null;

        public static bool IsAccountOnline(int accountId) => ChannelServers
            .Values
            .SelectMany(x => x.Clients.Values)
            .Any(x => x?.Account?.ID == accountId);

        public static MapleClient GetClientByCharacterId(int chrId) => ChannelServers
            .Values
            .SelectMany(x => x.Clients.Values)
            .Where(x => x?.Account?.Character?.ID == chrId)
            .FirstOrDefault(x => x != null);

		public static MapleClient GetClientByCharacterName(string ign) => ChannelServers
            .Values
			.SelectMany(x => x.Clients.Values)
			.Where(x => x?.Account?.Character?.Name?.ToLower() == ign)
			.FirstOrDefault(x => x != null);

		public static MapleCharacter GetCharacterById(int chrId) => ChannelServers
			.Values
			.SelectMany(x => x.Clients.Values)
            .Where(x => x?.Account?.Character?.ID == chrId)
            .Select(x => x.Account.Character)
			.FirstOrDefault(x => x != null);

		public static MapleClient GetClientByAccountId(int accountId) => ChannelServers
			.Values
			.SelectMany(x => x.Clients.Values)
            .Where(x => x?.Account?.ID == accountId)
			.FirstOrDefault(x => x != null);

        public static MapleCharacter GetCharacterByAccountId(int accountId) => GetClientByAccountId(accountId)?.Account.Character;

		public static MapleCharacter GetCharacterByName(string ign) => ChannelServers
			.Values
			.SelectMany(x => x.Clients.Values)
			.Where(x => x?.Account?.Character?.Name?.ToLower() == ign)
            .Select(x => x.Account.Character)
			.FirstOrDefault(x => x != null);

        public static Dictionary<MapleClient, bool> GetOnlineBuddies(List<int> characterIds, List<int> accountIds)
        {
            var onlineBuddies = new Dictionary<MapleClient, bool>();

            if (accountIds.Count == 0 && characterIds.Count == 0)
            {
                return onlineBuddies;
            }

            ChannelServers
                .Values
                .SelectMany(x => x.Clients.Values)
                .Where(x => x.Account?.Character != null)
                .ToList()
                .ForEach(c =>
                {
                    if (onlineBuddies.ContainsKey(c))
                    {
                        return;
                    }

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
                });

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
            Events.TryGetValue(id, out var ret);
            return ret;
        }

        public static void UnregisterEvent(int eventId) => Events.Remove(eventId);

        public static async Task Configure()
        {
			await ServerConfig.LoadFromFile("ServerConfig.json");
			MapleClient.RegisterPacketHandlers();
			LoadMobs();
			LoadMaps();
			InitializeDatabase();
			InitializeLoginServer();
			InitializeChannelServers();
        }

		public static void LoadMaps()
		{
			var sw = Stopwatch.StartNew();
			int count = DataProvider.LoadMaps(Path.Combine(ServerConfig.Instance.WzFilePath, "Map.wz"));
			Log.LogInformation($"{count} Maps loaded in {sw.ElapsedMilliseconds} ms");
			sw.Stop();
		}

		public static void LoadMobs()
		{
			var sw = Stopwatch.StartNew();
			int count = DataProvider.LoadMobs(Path.Combine(ServerConfig.Instance.WzFilePath, "Mob.wz"));
			Log.LogInformation($"{count} Mobs loaded in {sw.ElapsedMilliseconds} ms");
			sw.Stop();
		}

		private static void InitializeChannelServers()
		{
			for (var i = 0; i < ServerConfig.Instance.Channels; i++)
			{
				var channelServer = new ChannelServer((ushort)(ServerConfig.Instance.ChannelStartPort + i));
				ServerManager.ChannelServers[i] = channelServer;
			}
		}

		private static void InitializeLoginServer()
		{
			LoginServer = new LoginServer();
		}

		private static void InitializeDatabase()
		{
			Log.LogInformation("Initializing Database");

			using (var context = new MapleDbContext())
			{
				var accounts = context.Accounts.ToArray();
			}
		}
    }
}