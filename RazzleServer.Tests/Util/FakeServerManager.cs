using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game;
using RazzleServer.Game.Server;
using RazzleServer.Login;
using Serilog;

namespace RazzleServer.Tests.Util
{
    public class FakeServerManager : IServerManager
    {
        public ILoginServer Login { get; set; }
        public Worlds Worlds { get; } = new Worlds();
        public IShopServer Shop { get; set; }
        public Migrations Migrations { get; } = new Migrations();

        public void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .Console()
                .CreateLogger();

            var config = ServerConfig.Instance;
            config.DatabaseConnectionType = DatabaseConnectionType.InMemory;
            config.DatabaseConnection = Guid.NewGuid().ToString();
            config.AddDefaultWorld();

            Login = new LoginServer(this);

            ServerConfig.Instance.Worlds.ForEach(x =>
            {
                var world = new World(x);
                Worlds.Add(world);

                for (byte i = 0; i < x.Channels; i++)
                {
                    var game = new GameServer(this, world, ServerConfig.Instance.ChannelPort++, i);
                    world.Add(game);
                }
            });
        }

        public FakeLoginClient AddFakeLoginClient() => new FakeLoginClient(Login as LoginServer);

        public int ValidateMigration(string host, int characterId) => characterId;

        public void Migrate(string host, int accountId, int characterId)
        {
        }
    }
}
