using RazzleServer.Common.Util;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Login;
using RazzleServer.Game;
using RazzleServer.Center.Maple;
using RazzleServer.Server;
using RazzleServer.Game.Maple.Data;
using System;
using RazzleServer.Common.Data;

namespace RazzleServer.Center
{
    public class ServerManager
    {
        private static ServerManager _instance;

        public LoginServer Login { get; set; }
        public Worlds Worlds { get; private set; } = new Worlds();
        public Maple.Migrations Migrations { get; private set; } = new Maple.Migrations();
        public IServiceProvider ServiceProvider { get; set; }

        private readonly ILogger Log = LogManager.Log;

        public async Task Configure()
        {
            await ServerConfig.LoadFromFile("ServerConfig.json");
            DataProvider.Initialize();
            InitializeDatabase();
            LoginServer.RegisterPacketHandlers();
            GameServer.RegisterPacketHandlers();
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
            Log.LogInformation("Initializing Database");

            using (var context = new MapleDbContext())
            {
                var accounts = context.Accounts.ToArray();
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

        public static ServerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerManager();
                }
                return _instance;
            }
        }
    }
}