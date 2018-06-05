using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class NpcScripts
    {
        private readonly ILogger _log = LogManager.Log;

        public Dictionary<string, Type> Data { get; set; } = new Dictionary<string, Type>();

        public void Execute(Npc npc, Character character)
        {
            try
            {
                var script = npc.CachedReference.Script ?? npc.MapleId.ToString();

                if (!Data.ContainsKey(script))
                {
                    _log.LogWarning($"Script not implemented for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                    return;
                }

                var npcScript = Activator.CreateInstance(Data[script]) as ANpcScript;
                npcScript.Character = character;
                npcScript.Npc = npc;
                character.NpcScript = npcScript;
                Task.Factory.StartNew(npcScript.Execute);
            }
            catch (NotImplementedException)
            {
                _log.LogWarning($"Script not implemented  for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Script error for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
            }
        }
    }
}
