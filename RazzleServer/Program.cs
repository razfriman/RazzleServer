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

            System.Console.WriteLine("ARGS: " + string.Join(", ", args));

            if (args.Length == 0)
            {
                var center = new CenterServer();
                var login = new LoginServer();
            }
            if (args[0] == "C")
            {
                var center = new CenterServer();
            }
            else if (args[0] == "L")
            {
                var login = new LoginServer();
            }

            ServerConfig.Instance.Worlds.ForEach(x =>
            {
                //var game = new GameServer(x);
            });

            Thread.Sleep(Timeout.Infinite);
        }
    }
}