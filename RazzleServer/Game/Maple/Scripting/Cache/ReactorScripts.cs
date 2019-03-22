using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class ReactorScripts
    {
        private readonly ILogger _log = LogManager.CreateLogger<ReactorScripts>();

        public Dictionary<string, Type> Data { get; set; } = new Dictionary<string, Type>();

        public void Execute(Reactor reactor, Character character)
        {
            var script = reactor.CachedReference.Script ?? reactor.MapleId.ToString();

            if (!Data.ContainsKey(script))
            {
                _log.LogWarning($"Script not implemented for Reactor={reactor.MapleId} Script={script} on Map={reactor.Map.MapleId}");
                return;
            }

            var reactorScript = Activator.CreateInstance(Data[script]) as AReactorScript;
            reactorScript.Character = character;
            reactorScript.Reactor = reactor;
            Task.Factory.StartNew(reactorScript.Execute)
            .ContinueWith(x =>
            {
                var ex = x.Exception.Flatten().InnerException;

                if (ex is NotImplementedException)
                {
                    _log.LogWarning($"Script not implemented for Reactor={reactor.MapleId} Script={script} on Map={reactor.Map.MapleId}");
                }
                else
                {
                    _log.LogError(ex, $"Script error for Reactor={reactor.MapleId} on Map={reactor.Map.MapleId}");
                }

            }, TaskContinuationOptions.OnlyOnFaulted);
            Task.Factory.StartNew(reactorScript.Execute);
        }
    }
}
