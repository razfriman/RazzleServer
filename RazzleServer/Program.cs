using System;
using System.Threading.Tasks;
using RazzleServer.Center;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var manager = ServerManager.Instance;
            await manager.Configure();
            manager.Start();
            manager.ProcessInput();
        }
    }
}