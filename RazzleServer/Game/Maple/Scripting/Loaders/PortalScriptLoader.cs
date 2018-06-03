using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class PortalScriptLoader : AScriptLoader<PortalScripts>
    {
        public override string CacheName => "Portal Scripts";

        public override Task LoadScripts()
        {
            var types = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(APortalScript)))
                .ToList();

            types.ForEach(type =>
            {
                type
                    .GetTypeInfo()
                    .GetCustomAttributes()
                    .OfType<PortalScriptAttribute>()
                    .ToList()
                    .ForEach(x => Data.Data[x.Script] = type);
            });

            return Task.CompletedTask;
        }
    }
}