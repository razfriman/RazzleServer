using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Common.Util
{
    public static class LogManager
    {
        public static readonly ILoggerFactory Factory = new LoggerFactory()
            .AddFile("Logs/RazzleServer-{Date}.txt");

        public static ILogger Log => Factory.CreateLogger("LoggerNeedsMigration");
        
        public static IServiceProvider ServiceProvider { get; set; }

        public static ILogger CreateLogger<T>() => ServiceProvider.GetService<ILogger<T>>();
    }
}
