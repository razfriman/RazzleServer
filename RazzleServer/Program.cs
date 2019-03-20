using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazzleServer.Common;
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
            PrepareLogger();
            
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureHostConfiguration((config) => { config.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration((config) =>
                {
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostContext, configLogging) => configLogging.AddSerilog(dispose: true))
                .ConfigureServices((hostContext, services) => { services.AddHostedService<ServerManager>(); })
                .Build();

            await host.RunAsync();
        }

        private static void PrepareLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource("Microsoft"))
                    .WriteTo.File(new CompactJsonFormatter(),"Logs/Microsoft.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .WriteTo.File(new CompactJsonFormatter(), "Logs/RazzleServer.log", rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }
    }
}
