using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Scripts.Loaders;

namespace RazzleServer.Game.Maple.Scripts
{

    public static class ScriptProvider
    {
        public static CommandScripts Commands { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static void Initialize()
        {
            var sw = new Stopwatch();

            sw.Start();

            Log.LogInformation("Loading scripts...");

            Task.WaitAll(
                Task.Run(async () => Commands = await new CommandScriptLoader().Load())
            );

            sw.Stop();

            Log.LogInformation("Scripts loaded in {0}ms.", sw.ElapsedMilliseconds);
        }
    }
}
