using RazzleServer.Common.Server;

namespace RazzleServer.Server
{
    public interface IServerManager
    {
        ILoginServer Login { get; set; }
        AWorlds Worlds { get; }
        IShopServer Shop { get; set; }
        Migrations Migrations { get; }
        int ValidateMigration(string host, int characterId);
        void Migrate(string host, int accountId, int characterId);
    }
}
