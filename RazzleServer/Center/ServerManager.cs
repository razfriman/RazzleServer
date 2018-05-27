using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Login;

namespace RazzleServer.Center
{
    public class ServerManager
    {
        private static ServerManager _instance;

        public LoginServer Login { get; set; }
        public Worlds Worlds { get; } = new Worlds();
        public Migrations Migrations { get; } = new Migrations();

        private readonly ILogger _log = LogManager.Log;

        public async Task Configure()
        {
            await ServerConfig.LoadFromFile("ServerConfig.json");
            DataProvider.Initialize();
            InitializeDatabase();
        }

        public void Start()
        {
            var login = new LoginServer(this);

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

        private void InitializeDatabase()
        {
            _log.LogInformation("Initializing Database");

            using (var context = new MapleDbContext())
            {
                context.Database.EnsureCreated();
                var _ = context.Accounts.ToArray();
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

        internal void ProcessInput()
        {
            while (true)
            {
                var line = Console.ReadLine();
                ProcessCommandLineComand(line);
            }
        }

        private void ProcessCommandLineComand(string line)
        {
            Console.WriteLine($"Unknown command: {line}");
        }

        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());
    }
}