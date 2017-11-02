using RazzleServer.Common.Util;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Data;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Server
{
    public class ServerManager
    {
        private readonly ILogger Log = LogManager.Log;

        public async Task Configure()
        {
			await ServerConfig.LoadFromFile("ServerConfig.json");
            InitializeDatabase();
        }

		private void InitializeDatabase()
		{
			Log.LogInformation("Initializing Database");

			using (var context = new MapleDbContext())
			{
				var accounts = context.Accounts.ToArray();
			}
		}
    }
}