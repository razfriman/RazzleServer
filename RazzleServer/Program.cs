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
            await ServerManager.Configure();

            Task.Factory.StartNew(() => new CenterServer());
            Task.Factory.StartNew(() => new LoginServer());

            ServerConfig.Instance.Worlds.ForEach(x => {
                Task.Factory.StartNew(() => new GameServer(x.Port));
            });

            Thread.Sleep(Timeout.Infinite);
        }
    }
}