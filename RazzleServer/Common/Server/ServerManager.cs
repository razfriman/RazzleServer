using RazzleServer.Common.Util;
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
		}

		public static void LoadMobs()
		{
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