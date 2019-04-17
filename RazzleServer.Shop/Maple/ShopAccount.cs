using RazzleServer.Game.Server;

namespace RazzleServer.Shop.Maple
{
    public sealed class ShopAccount : AMapleAccount
    {
        public ShopClient Client { get; }
        public ShopAccount(ShopClient client) => Client = client;

        public ShopAccount(int accountId, ShopClient client) : base(accountId) => Client = client;
    }
}
