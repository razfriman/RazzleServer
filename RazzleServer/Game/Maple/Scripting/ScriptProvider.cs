using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Scripting.Cache;
using RazzleServer.Game.Maple.Scripting.Loaders;

namespace RazzleServer.Game.Maple.Scripting
{
    public static class ScriptProvider
    {
        public static CommandScripts Commands { get; private set; }
        public static NpcScripts Npcs { get; private set; }
        public static ReactorScripts Reactors { get; private set; }
        public static PortalScripts Portals { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static async Task Initialize()
        {
            var sw = new Stopwatch();

            sw.Start();

            await Task.WhenAll(
                Task.Run(async () => Commands = await new CommandScriptLoader().Load()),
                Task.Run(async () => Portals = await new PortalScriptLoader().Load()),
                Task.Run(async () => Npcs = await new NpcScriptLoader().Load()),
                Task.Run(async () => Reactors = await new ReactorScriptLoader().Load())
            );

            sw.Stop();

            Log.LogInformation("Scripts loaded in {0}ms.", sw.ElapsedMilliseconds);
        }
    }
}
