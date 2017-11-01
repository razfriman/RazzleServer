using System.Net;
using RazzleServer.Center.Maple;
using RazzleServer.Server;

namespace RazzleServer.Center
{
    public class CenterServer : MapleServer<CenterClient>
    {
        public CenterClient Login { get; set; }
        public Worlds Worlds { get; private set; }
        public Maple.Migrations Migrations { get; private set; }

        public CenterServer()
        {
            Worlds = new Worlds();
            Migrations = new Maple.Migrations();
            Start(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
        }
    }
}
