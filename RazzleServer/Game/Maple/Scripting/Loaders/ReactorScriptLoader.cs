using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class ReactorScriptLoader : AScriptLoader<ReactorScripts>
    {
        public override string CacheName => "Reactors";

        public override Task LoadScripts()
        {
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(AReactorScript)))
                .ToList()
                .ForEach(x => Data.Add((AReactorScript)Activator.CreateInstance(x)));

            return Task.CompletedTask;
        }
    }
}