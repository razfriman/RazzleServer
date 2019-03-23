using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazzleServer.Common;
using RazzleServer.Common.Server;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace RazzleServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }

                    var configuration = config.Build();

                    ServerConfig.Load(configuration);
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();
                })
                .ConfigureLogging((hostContext, configLogging) => configLogging.AddSerilog(dispose: true))
                .ConfigureServices((hostContext, services) => { services.AddHostedService<ServerManager>(); })
                .Build();

            await host.RunAsync();
        }
    }
}
