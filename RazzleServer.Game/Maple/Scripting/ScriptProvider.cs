﻿using System.Diagnostics;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;
using RazzleServer.Game.Maple.Scripting.Loaders;
using Serilog;

namespace RazzleServer.Game.Maple.Scripting
{
    public class ScriptProvider
    {
        public static CommandScripts Commands { get; private set; }
        public static NpcScripts Npcs { get; private set; }

        private static readonly ILogger Logger = Log.ForContext<ScriptProvider>();

        public static async Task Initialize()
        {
            var sw = Stopwatch.StartNew();

            await Task.WhenAll(
                Task.Run(async () => Commands = await new CommandScriptLoader().Load()),
                Task.Run(async () => Npcs = await new NpcScriptLoader().Load())
            );

            sw.Stop();

            Logger.Information("Scripts loaded in {0}ms.", sw.ElapsedMilliseconds);
        }
    }
}
