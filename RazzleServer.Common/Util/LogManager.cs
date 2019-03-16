using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Common.Util
{
    public static class LogManager
    {
        public static readonly ILoggerFactory Factory = new LoggerFactory()
            .AddFile("Logs/RazzleServer-{Date}.txt");

        public static ILogger Log => Factory.CreateLogger(new StackFrame(1, false).GetMethod().DeclaringType.FullName);

        public static ILogger LogByName(string fullName) => Factory.CreateLogger(fullName);
    }
}