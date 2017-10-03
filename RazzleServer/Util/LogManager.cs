using Microsoft.Extensions.Logging;

namespace RazzleServer.Util
{
    public static class LogManager
    {
        private static ILogger _log;

        public static ILogger Log
        {
            get
            {
				if (_log == null)
				{
					var factory = new LoggerFactory()
						.AddConsole()
						.AddDebug();

					_log = factory.CreateLogger("MapleLib");
				}

				return _log;
            }
        }
    }
}