using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Server;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public abstract class ACachedDataLoader<T> where T : new()
    {
        public abstract string CacheName { get; }

        public abstract ILogger Log { get; }

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
                    Log.LogError(e, $"Error loading [{CacheName}] cache. Attempting to load from WZ");
                    LoadFromWz();
                    await SaveToCache();
                }
            }
            else
            {
                Log.LogInformation($"[{CacheName}] cache not found. Attempting to load from WZ");
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
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Formatting = ServerConfig.Instance.PrettifyCache ? Formatting.Indented : Formatting.None
                };
                serializer.Serialize(writer, Data);
            }

            Log.LogInformation($"Saving [{CacheName}] to cached file");
            return Task.CompletedTask;
        }

        public virtual Task LoadFromCache()
        {

            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");

            using (var s = File.OpenRead(path))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                Data = serializer.Deserialize<T>(reader);
                Log.LogInformation($"Loaded [{CacheName}] from cache");
            }

            return Task.CompletedTask;
        }

        public abstract void LoadFromWz();

        public WzFile GetWzFile(string name) => new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, name), (short) ServerConfig.Instance.Version, WzMapleVersionType.Classic);
    }
}
