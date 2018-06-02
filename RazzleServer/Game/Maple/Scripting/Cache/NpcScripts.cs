using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Scripts.Cache
{
    public sealed class NpcScripts : MapleKeyedCollection<string, ANpcScript>
    {
        private readonly ILogger _log = LogManager.Log;

        public override string GetKey(ANpcScript item) => item.Name;

        public void Execute(Npc npc, Character character)
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
