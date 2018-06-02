using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
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