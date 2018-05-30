using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public abstract class ACachedDataLoader<T> where T : new()
    {
        private readonly ILogger _log;

        public abstract string CacheName { get; }

        public T Data { get; private set; } = new T();

        protected ACachedDataLoader()
        {
            _log = LogManager.LogByName(GetType().FullName);
        }

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
                _log.LogInformation($"[{CacheName}] cache not found, loading from WZ");
                LoadFromWz();
                await SaveToCache();
            }

            return Data;
        }

        public virtual Task SaveToCache()
        {
            Directory.CreateDirectory(ServerConfig.Instance.CacheFolder);
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");

            using (var s = File.OpenWrite(path))
            using (var sr = new StreamWriter(s))
            using (var writer = new JsonTextWriter(sr))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = ServerConfig.Instance.PrettifyCache ? Formatting.Indented : Formatting.None
                };
                serializer.Serialize(writer, Data);
            }

            _log.LogInformation($"Saving [{CacheName}] to cached file");
            return Task.CompletedTask;
        }

        public virtual Task LoadFromCache()
        {
            _log.LogInformation($"Loading [{CacheName}] from cache");
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");

            using (var s = File.OpenRead(path))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                Data = serializer.Deserialize<T>(reader);
            }

            return Task.CompletedTask;;
        }

        public abstract void LoadFromWz();

        public WzFile GetWzFile(string name) => new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, name), WzMapleVersion.Gms);
    }
}