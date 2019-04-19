using System.Net.Sockets;
using RazzleServer.Net;

namespace RazzleServer.Game.Server
{
    public abstract class AMapleClient : AClient
    {
        protected AMapleClient(Socket session, ushort version, byte subVersion, byte serverType, ulong? aesKey,
            bool printPackets, bool toClient) : base(session, version, subVersion, serverType,
            aesKey, printPackets, toClient)
        {
        }

        public AMapleAccount Account { get; set; }
        public abstract ILoginServer LoginServer { get; }
        public abstract IGameServer GameServer { get; }
        public abstract IShopServer ShopServer { get; }
    }
}
