using RazzleServer.Server;
using System.Threading;
using System.Threading.Tasks;
using RazzleServer.Center;
using RazzleServer.Login;
using RazzleServer.Game;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var manager = new ServerManager();
            await manager.Configure();

            var center = new CenterServer();
            var login = new LoginServer();

            ServerConfig.Instance.Worlds.ForEach(x =>
            {
                //var game = new GameServer(x);
            });

            Thread.Sleep(Timeout.Infinite);
        }
    }
}