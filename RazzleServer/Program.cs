using RazzleServer.Server;
using System.Threading;
using System.Threading.Tasks;
using RazzleServer.Center;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var manager = ServerManager.Instance;
            await manager.Configure();

            //manager.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}