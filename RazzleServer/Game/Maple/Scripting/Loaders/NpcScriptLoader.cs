using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripts.Cache;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Scripts.Loaders
{
    public class NpcScriptLoader : AScriptLoader<NpcScripts>
    {
        public override string CacheName => "Npcs";

        public override Task LoadScripts()
        {
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(ANpcScript)))
                .ToList()
                .ForEach(x => Data.Add((ANpcScript)Activator.CreateInstance(x)));

            return Task.CompletedTask;
        }
    }
}