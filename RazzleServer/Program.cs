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
            Thread.Sleep(Timeout.Infinite);
        }
    }
}