using System.Net.Sockets;
using RazzleServer.Net;

namespace RazzleServer.Game
{
    public abstract class AMapleClient : AClient
    {
        protected AMapleClient(Socket session, ushort version, byte subVersion, byte serverType, ulong aesKey, bool useAesEncryption, bool printPackets, bool toClient) : base(session, version, subVersion, serverType, aesKey, useAesEncryption, printPackets, toClient)
        {
        }
        
        public ILoginServer LoginServer { get; set; }
        public IGameServer GameServer { get; set; }
        public IShopServer ShopServer { get; set; }
    }
}
