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

        public static void Main(string[] args)
        {
            ServerConfig.LoadFromFile("ServerConfig.json");

            LoadMobs();
            LoadMaps();

            MapleClient.RegisterPacketHandlers();

            Log.Info("Initializing Database");
            using (var context = new MapleDbContext())
            {
                var accounts = context.Accounts.ToArray();
            }

            ServerManager.LoginServer = new LoginServer();

            for (var i = 0; i < ServerConfig.Instance.Channels; i++)
            {
                var channelServer = new ChannelServer((ushort)(ServerConfig.Instance.ChannelStartPort + i));
                ServerManager.ChannelServers[i] = channelServer;
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
