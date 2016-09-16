using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Data;
using System.Linq;
using NLog;
using System.Threading;
using System.Diagnostics;

namespace RazzleServer
{
    public class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
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
            int count = DataProvider.LoadMaps(@"C:\Nexon\MapleStoryV83\Map.wz");
            Log.Info($"{count} Maps loaded in {sw.ElapsedMilliseconds} ms");
            sw.Stop();
        }
        public static void LoadMobs()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int count = DataProvider.LoadMobs(@"C:\Nexon\MapleStoryV83\Mob.wz");
            Log.Info($"{count} Mobs loaded in {sw.ElapsedMilliseconds} ms");
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
            Log.Info("Initializing Database");
            using (var context = new MapleDbContext())
            {
                var accounts = context.Accounts.ToArray();
            }
        }
    }
}