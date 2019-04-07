using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazzleServer.Common.Constants;
using RazzleServer.Game;
using RazzleServer.Login;
using Serilog;

namespace RazzleServer.Common
{
    public class FakeServerManager : AServerManager
    {
        public override Task Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .Console()
                .CreateLogger();

            var config = ServerConfig.Instance;
            config.DatabaseConnectionType = DatabaseConnectionType.InMemory;
            config.AddDefaultWorld();
            return base.Configure();
        }
        
        public override async Task StartAsync(CancellationToken cancellationToken)
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

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public TestLoginClient AddFakeLoginClient() => new TestLoginClient(Login);
    }
}
