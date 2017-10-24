using System.Net;
using RazzleServer.Center.Maple;
using RazzleServer.Server;

namespace RazzleServer.Center
{
    public class WvsCenter : MapleServer
    {
        public static CenterClient Login { get; set; }
        public static Worlds Worlds { get; private set; }
        public static Maple.Migrations Migrations { get; private set; }

        public WvsCenter()
        {
            Worlds = new Worlds();
            Migrations = new Maple.Migrations();
            byte[] listenIp = { 0, 0, 0, 0 };
            Start(new IPAddress(listenIp), ServerConfig.Instance.CenterPort);
        }
    }
}
