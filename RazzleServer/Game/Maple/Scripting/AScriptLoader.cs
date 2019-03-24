using System.Threading.Tasks;
using Serilog;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class AScriptLoader<T> where T : new()
    {
        protected readonly ILogger Logger;

        public abstract string CacheName { get; }

        public T Data { get; } = new T();

        protected AScriptLoader() => Logger = Log.ForContext<AScriptLoader<T>>();

        public virtual async Task<T> Load()
        {
            await LoadScripts();
            Log.Information($"Loaded [{CacheName}]");
            return Data;
        }

        public abstract Task LoadScripts();
    }
}
