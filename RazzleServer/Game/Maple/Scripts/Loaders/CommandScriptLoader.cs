using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RazzleServer.Game.Maple.Scripts.Loaders
{
    public class CommandScriptLoader : AScriptLoader<CommandScripts>
    {
        public override string CacheName => "Commands";

        public override Task LoadScripts()
        {
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(ACommandScript)))
                .ToList()
                .ForEach(x => Data.Add((ACommandScript)Activator.CreateInstance(x)));

            return Task.CompletedTask;
        }
    }
}