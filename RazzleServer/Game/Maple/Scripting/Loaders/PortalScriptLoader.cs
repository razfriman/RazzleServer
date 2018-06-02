using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripts.Cache;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Scripts.Loaders
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