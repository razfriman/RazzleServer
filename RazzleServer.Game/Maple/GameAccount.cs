using RazzleServer.Game.Server;

namespace RazzleServer.Game.Maple
{
    public sealed class GameAccount : AMapleAccount
    {
        public GameClient Client { get; }
        public GameAccount(int accountId, GameClient client) : base(accountId) => Client = client;
    }
}
