using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Center;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public abstract class ACachedDataLoader<T> where T : new()
    {
        private readonly ILogger _log = LogManager.Log;

        public abstract string CacheName { get; }

        public T Data { get; private set; } = new T();

        public virtual async Task<T> Load()
        {
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");
            if (File.Exists(path))
            {
                try
                {
                    await LoadFromCache();
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error loading cache data. Attempting to load from WZ");
                    LoadFromWz();
                    await SaveToCache();
                }
            }
            else
            {
                _log.LogInformation($"{CacheName} cache not found, loading from WZ");
                LoadFromWz();
                await SaveToCache();
            }

            return Data;
        }

        public virtual async Task SaveToCache()
        {
            Directory.CreateDirectory(ServerConfig.Instance.CacheFolder);
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(Data, Formatting.Indented));
            _log.LogInformation($"Saving {CacheName} to cached file");
        }

        public virtual async Task LoadFromCache()
        {
            _log.LogInformation($"Loading {CacheName} from cache");
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");
            var contents = await File.ReadAllTextAsync(path);
            Data = JsonConvert.DeserializeObject<T>(contents);
        }

        public abstract void LoadFromWz();
    }
}