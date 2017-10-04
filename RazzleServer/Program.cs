using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Data;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Util;
using System.Threading.Tasks;

namespace RazzleServer
{
    public static class Program
    {
        private static readonly ILogger Log = LogManager.Log;

        public static async Task Main(string[] args)
        {
            ServerConfig.LoadFromFile("ServerConfig.json");
            MapleClient.RegisterPacketHandlers();
            LoadMobs();
            LoadMaps();
            InitializeDatabase();
            InitializeLoginServer();
            InitializeChannelServers();

            Thread.Sleep(Timeout.Infinite);
        }

        public static void LoadMaps()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int count = DataProvider.LoadMaps(Path.Combine(ServerConfig.Instance.WzFilePath, "Map.wz"));
            Log.LogInformation($"{count} Maps loaded in {sw.ElapsedMilliseconds} ms");
            sw.Stop();
        }

        public static void LoadMobs()
        {
            Stopwatch sw = Stopwatch.StartNew();
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
            ServerManager.LoginServer = new LoginServer();
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