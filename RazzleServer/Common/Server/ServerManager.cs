using RazzleServer.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.IO;
using RazzleServer.Game;
using RazzleServer.Login;

namespace RazzleServer.Server
{
    public static class ServerManager
    {
        private static readonly ILogger Log = LogManager.Log;

        public static async Task Configure()
        {
			await ServerConfig.LoadFromFile("ServerConfig.json");
			LoginClient.RegisterPacketHandlers();
            GameClient.RegisterPacketHandlers();
			LoadMobs();
			LoadMaps();
			InitializeDatabase();
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