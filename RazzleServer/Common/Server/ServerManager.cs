using RazzleServer.Common.Util;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Data;
using Microsoft.Extensions.Logging;
using RazzleServer.Game;
using RazzleServer.Login;
using RazzleServer.Center;

namespace RazzleServer.Server
{
    public static class ServerManager
    {
        private static readonly ILogger Log = LogManager.Log;

        public static async Task Configure()
        {
			await ServerConfig.LoadFromFile("ServerConfig.json");
            CenterClient.RegisterPacketHandlers();
			LoginClient.RegisterPacketHandlers();
            GameClient.RegisterPacketHandlers();
            InitializeDatabase();
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