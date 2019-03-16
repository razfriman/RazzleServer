using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RazzleServer.Common;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureHostConfiguration((config) => { config.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration((config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddFilter((x, y) => true);
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                    configLogging.AddFile("Logs/RazzleServer-{Date}.txt");
                })
                .ConfigureServices((hostContext, services) => { services.AddHostedService<ServerManager>(); })
                .Build();

            await host.RunAsync();
        }
    }
}
