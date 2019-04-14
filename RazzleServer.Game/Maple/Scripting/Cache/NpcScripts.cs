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

        public void Execute(Npc npc, GameCharacter gameCharacter)
        {
            var script = npc.CachedReference.Script ?? npc.MapleId.ToString();

            if (!Data.ContainsKey(script))
            {
                _log.Warning(
                    $"Script not implemented for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                return;
            }

            if (!(Activator.CreateInstance(Data[script]) is ANpcScript npcScript))
            {
                _log.Warning(
                    $"Cannot instantiate script for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                gameCharacter.Release();   
                return;
            }
            
            npcScript.GameCharacter = gameCharacter;
            npcScript.Npc = npc;
            gameCharacter.NpcScript = npcScript;
            Task.Factory.StartNew(npcScript.Execute)
                .ContinueWith(x =>
                {
                    var ex = x.Exception?.Flatten().InnerException;

                    if (ex is NotImplementedException)
                    {
                        _log.Warning(
                            $"Script not implemented for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                        gameCharacter.Release();
                    }
                    else
                    {
                        _log.Error(ex,
                            $"Script error for Npc={npc.MapleId} Script={npc.CachedReference.Script} on Map={npc.Map.MapleId}");
                        gameCharacter.Release();
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
