using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class AScriptLoader<T> where T : new()
    {
        protected readonly ILogger Log;

        public abstract string CacheName { get; }

        public T Data { get; } = new T();

        protected AScriptLoader() => Log = LogManager.CreateLogger<AScriptLoader<T>>();

        public virtual async Task<T> Load()
        {
            await LoadScripts();
            Log.LogInformation($"Loaded [{CacheName}]");
            return Data;
        }

        public abstract Task LoadScripts();
    }
}
