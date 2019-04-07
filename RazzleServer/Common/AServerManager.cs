using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RazzleServer.Common.Server;
using RazzleServer.Game;
using RazzleServer.Login;
using Serilog;

namespace RazzleServer.Common
{
    public abstract class AServerManager : IHostedService
    {
        public LoginServer Login { get; set; }
        public Worlds Worlds { get; } = new Worlds();
        public Migrations Migrations { get; } = new Migrations();

        private readonly ILogger _log = Log.ForContext<AServerManager>();

        public virtual Task Configure()
        {
            InitializeDatabase();
            return Task.CompletedTask;
        }

        private void InitializeDatabase()
        {
            _log.Information($"Initializing Database Type={ServerConfig.Instance.DatabaseConnectionType} Connection={ServerConfig.Instance.DatabaseConnection}");
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
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Login.Shutdown();
            Worlds.Values.SelectMany(x => x.Values).ToList().ForEach(x => x.Shutdown());
            return Task.CompletedTask;
        }
    }
}
