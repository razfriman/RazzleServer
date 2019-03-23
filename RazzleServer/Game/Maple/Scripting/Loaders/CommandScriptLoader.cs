using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Scripting.Cache;

namespace RazzleServer.Game.Maple.Scripting.Loaders
{
    public class CommandScriptLoader : AScriptLoader<CommandScripts>
    {
        public override string CacheName => "Command Scripts";

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
