using RazzleServer.Server;
using System.Threading;
using System.Threading.Tasks;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await ServerManager.Configure();

            var center = new Center.CenterServer();
            var login = new Login.LoginServer();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}