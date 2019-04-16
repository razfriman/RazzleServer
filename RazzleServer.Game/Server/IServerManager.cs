namespace RazzleServer.Game.Server
{
    public interface IServerManager
    {
        ILoginServer Login { get; set; }
        Worlds Worlds { get; }
        IShopServer Shop { get; set; }
        Migrations Migrations { get; }
        int ValidateMigration(string host, int characterId);
        void Migrate(string host, int accountId, int characterId);
    }
}
