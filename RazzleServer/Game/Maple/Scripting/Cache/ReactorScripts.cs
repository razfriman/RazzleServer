using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class ReactorScripts : MapleKeyedCollection<string, AReactorScript>
    {
        private readonly ILogger _log = LogManager.Log;

        public override string GetKey(AReactorScript item) => item.Name;

        public void Execute(Reactor reactor, Character character)
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
