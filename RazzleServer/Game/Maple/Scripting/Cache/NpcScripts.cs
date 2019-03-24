using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using Serilog;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class NpcScripts
    {
        private readonly ILogger _log = Log.ForContext<NpcScripts>();

        public Dictionary<string, Type> Data { get; set; } = new Dictionary<string, Type>();

        public void Execute(Npc npc, Character character)
        {
            var script = npc.CachedReference.Script ?? npc.MapleId.ToString();

            if (!Data.ContainsKey(script))
            {
                _log.Warning(
                    $"Script not implemented for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                return;
            }

            var npcScript = Activator.CreateInstance(Data[script]) as ANpcScript;
            npcScript.Character = character;
            npcScript.Npc = npc;
            character.NpcScript = npcScript;
            Task.Factory.StartNew(npcScript.Execute)
                .ContinueWith(x =>
                {
                    var ex = x.Exception?.Flatten().InnerException;

                    if (ex is NotImplementedException)
                    {
                        _log.Warning(
                            $"Script not implemented for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                        character.Release();
                    }
                    else
                    {
                        _log.Error(ex,
                            $"Script error for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                        character.Release();
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
