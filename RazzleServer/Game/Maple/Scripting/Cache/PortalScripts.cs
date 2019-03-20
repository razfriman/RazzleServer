using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class PortalScripts
    {
        private readonly ILogger _log = LogManager.CreateLogger<PortalScripts>();

        public Dictionary<string, Type> Data { get; set; } = new Dictionary<string, Type>();

        public void Execute(Portal portal, Character character)
        {
            if (!Data.ContainsKey(portal.Script))
            {
                _log.LogWarning($"Script not implemented for Portal={portal.Label} Script={portal.Script} on Map={portal.Map.MapleId}");
                return;
            }

            var portalScript = Activator.CreateInstance(Data[portal.Script]) as APortalScript;
            portalScript.Character = character;
            portalScript.Portal = portal;
            Task.Factory.StartNew(portalScript.Execute)
                .ContinueWith(x =>
                {
                    var ex = x.Exception.Flatten().InnerException;

                    if (ex is NotImplementedException)
                    {
                        _log.LogWarning($"Script not implemented for Portal={portal.Label} Script={portal.Script} on Map={portal.Map.MapleId}");
                    }
                    else
                    {
                        _log.LogError(ex, $"Script error for Portal={portal.Label} Script={portal.Script} on Map={portal.Map.MapleId}");
                    }

                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
