using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class PortalScriptLoader : AScriptLoader<PortalScripts>
    {
        public override string CacheName => "Portals";

        public override Task LoadScripts()
        {
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(APortalScript)))
                .ToList()
                .ForEach(x => Data.Add((APortalScript)Activator.CreateInstance(x)));

            return Task.CompletedTask;
        }
    }
}