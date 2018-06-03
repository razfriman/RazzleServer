using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class ReactorScripts
    {
        private readonly ILogger _log = LogManager.Log;

        public Dictionary<string, Type> Data { get; set; } = new Dictionary<string, Type>();

        public void Execute(Reactor reactor, Character character)
        {
            try
            {
                var script = reactor.Label;

                if (!Data.ContainsKey(script))
                {
                    _log.LogWarning($"Script not implemented for Reactor={reactor.Label} on Map={reactor.Map.MapleId}");
                    return;
                }

                var reactorScript = Activator.CreateInstance(Data[script]) as AReactorScript;
                reactorScript.Character = character;
                reactorScript.Reactor = reactor;
                Task.Factory.StartNew(reactorScript.Execute);
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Script error for Reactor={reactor.Label} on Map={reactor.Map.MapleId}");
            }
        }
    }
}
