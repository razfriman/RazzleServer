using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class NpcScriptLoader : AScriptLoader<NpcScripts>
    {
        public override string CacheName => "Npc Scripts";

        public override Task LoadScripts()
        {
            var types = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(ANpcScript)))
                .ToList();

            types.ForEach(type =>
            {
                type
                    .GetTypeInfo()
                    .GetCustomAttributes()
                    .OfType<NpcScriptAttribute>()
                    .ToList()
                    .ForEach(x => Data.Data[x.Script] = type);
            });
                
            return Task.CompletedTask;
        }
    }
}