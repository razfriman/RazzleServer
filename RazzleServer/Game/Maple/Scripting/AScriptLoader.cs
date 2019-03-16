using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class AScriptLoader<T> where T : new()
    {
        protected readonly ILogger _log;

        public abstract string CacheName { get; }

        public T Data { get; private set; } = new T();

        protected AScriptLoader()
        {
            _log = LogManager.LogByName(GetType().FullName);
        }

        public virtual async Task<T> Load()
        {
            await LoadScripts();
            _log.LogInformation($"Loaded [{CacheName}]");
            return Data;
        }

        public abstract Task LoadScripts();
    }
}
