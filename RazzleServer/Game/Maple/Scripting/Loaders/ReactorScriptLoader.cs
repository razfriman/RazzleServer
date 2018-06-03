using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class ReactorScriptLoader : AScriptLoader<ReactorScripts>
    {
        public override string CacheName => "Reactor Scripts";

        public override Task LoadScripts()
        {
            var types = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(AReactorScript)))
                .ToList();

            types.ForEach(type =>
            {
                type
                    .GetTypeInfo()
                    .GetCustomAttributes()
                    .OfType<ReactorScriptAttribute>()
                    .ToList()
                    .ForEach(x => Data.Data[x.Script] = type);
            });

            return Task.CompletedTask;
        }
    }
}