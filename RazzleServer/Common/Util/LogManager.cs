using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Common.Util
{
    public static class LogManager
    {
        private static readonly ILoggerFactory factory = new LoggerFactory()
            .AddConsole()
            .AddDebug();

        public static ILogger Log => factory.CreateLogger(new StackFrame(1, false).GetMethod().DeclaringType.FullName);
    }
}