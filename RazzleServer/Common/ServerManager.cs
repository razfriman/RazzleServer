using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Server;
using RazzleServer.Common.Util;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Login;

namespace RazzleServer.Common
{
    public class ServerManager : IHostedService
    {
        public LoginServer Login { get; set; }
        public Worlds Worlds { get; }
        public Migrations Migrations { get; }

        private readonly ILogger _logger;

        public ServerManager(ILogger<ServerManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            LogManager.ServiceProvider = serviceProvider;
            Worlds = new Worlds();
            Migrations = new Migrations();
            ;
        }

        public async Task Configure()
        {
            InitializeDatabase();
            await DataProvider.Initialize();
            await ScriptProvider.Initialize();
            await LootProvider.Initialize();
            await ShopProvider.Initialize();
        }

        private void InitializeDatabase()
        {
            _logger.LogInformation("Initializing Database");

            using (var context = new MapleDbContext())
            {
                context.Database.EnsureCreated();
            }
        }

        public int ValidateMigration(string host, int characterId) => Migrations.Validate(host, characterId);

        internal void Migrate(string host, int accountId, int characterId)
        {
            if (Migrations.Contains(host))
            {
                Migrations.Remove(host);
            }

            Migrations.Add(new Migration(host, accountId, characterId));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Configure();
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Login.Shutdown();
            Worlds.Values.SelectMany(x => x.Values).ToList().ForEach(x => x.Shutdown());
            return Task.CompletedTask;
        }
    }
}
