﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RazzleServer.Common;
using RazzleServer.Data;
using RazzleServer.DataProvider;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Login;
using RazzleServer.Shop;
using Serilog;

namespace RazzleServer
{
    public class ServerManager : IServerManager, IHostedService
    {
        public ILoginServer Login { get; set; }
        public AWorlds Worlds { get; } = new Worlds();
        public IShopServer Shop { get; set; }
        public Migrations Migrations { get; } = new Migrations();

        private readonly ILogger _log = Log.ForContext<ServerManager>();

        public async Task Configure()
        {
            InitializeDatabase();
            await CachedData.Initialize();
            await ScriptProvider.Initialize();
            await LootProvider.Initialize();
        }

        private void InitializeDatabase()
        {
            _log.Information(
                $"Initializing Database Type={ServerConfig.Instance.DatabaseConnectionType} Connection={ServerConfig.Instance.DatabaseConnection}");
            using var context = new MapleDbContext();
            context.Database.EnsureCreated();
        }

        public int ValidateMigration(string host, int characterId) => Migrations.Validate(host, characterId);

        public void Migrate(string host, int accountId, int characterId)
        {
            if (Migrations.Contains(host))
            {
                Migrations.Remove(host);
            }

            Migrations.Add(new Migration(host, accountId, characterId));
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await Configure();
            Login = new LoginServer(this);
            Login.Start();

            ServerConfig.Instance.Worlds.ForEach(x =>
            {
                var world = new World(x);
                Worlds.Add(world);

                for (byte i = 0; i < x.Channels; i++)
                {
                    var game = new GameServer(this, world, ServerConfig.Instance.ChannelPort++, i);
                    world.Add(game);
                    game.Start();
                }
            });

            Shop = new ShopServer(this);
            Shop.Start();
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Login.Shutdown();
            Worlds.Values.SelectMany(x => x.Values).ToList().ForEach(x => x.Shutdown());
            return Task.CompletedTask;
        }
    }
}
