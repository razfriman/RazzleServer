using System;
using System.Threading;
using System.Threading.Tasks;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game;
using RazzleServer.Login;
using RazzleServer.Server;
using Serilog;

namespace RazzleServer.Tests.Util
{
    public class FakeServerManager : IServerManager
    {
        public ILoginServer Login { get; set; }
        public AWorlds Worlds { get; }
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
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Configure();
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

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public FakeLoginClient AddFakeLoginClient() => new FakeLoginClient(Login as LoginServer);

        public int ValidateMigration(string host, int characterId)
        {
            return characterId;
        }

        public void Migrate(string host, int accountId, int characterId)
        {
        }
    }
}
