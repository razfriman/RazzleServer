using System.Collections.Generic;
using System.Net;
using RazzleServer.Net;

namespace RazzleServer.Game
{
    public interface IMapleServer
    {
        ushort Port { get; set; }

        void Shutdown();
        void Start(IPAddress ip, ushort port);

        Dictionary<string, AClient> Clients { get; set; }
    }
}
