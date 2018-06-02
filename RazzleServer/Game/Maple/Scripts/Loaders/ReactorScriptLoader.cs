using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Scripts.Loaders
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