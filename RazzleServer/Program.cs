using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Data;
using System.Linq;
using NLog;
using System.Threading;

namespace RazzleServer
{
    public class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            ServerConfig.LoadFromFile("ServerConfig.json");

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
