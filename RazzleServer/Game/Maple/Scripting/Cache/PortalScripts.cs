using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class PortalScripts : MapleKeyedCollection<string, APortalScript>
    {
        private readonly ILogger _log = LogManager.Log;

        public override string GetKey(APortalScript item) => item.Name;

        public void Execute(Portal portal, Character character)
        {
            try
            {
                // TODO - Call script
            }
            catch (Exception ex)
            {
                _log.LogError($"Script error: {ex}");
            }
        }
    }
}
