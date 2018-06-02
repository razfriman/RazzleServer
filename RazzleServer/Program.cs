using System.Threading.Tasks;
using RazzleServer.Common;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main()
        {
            var manager = ServerManager.Instance;
            await manager.Configure();
            manager.Start();
            manager.ProcessInput();
        }
    }
}