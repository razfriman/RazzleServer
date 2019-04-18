using RazzleServer.Common;
using RazzleServer.Game.Server;
using Serilog;

namespace RazzleServer.Shop
{
    public class ShopServer : MapleServer<ShopClient, ShopPacketHandler>, IShopServer
    {
        public ShopServer(IServerManager manager) : base(manager) => Port = ServerConfig.Instance.ShopPort;

        public override ILogger Logger => Log.ForContext<ShopServer>();
    }
}
