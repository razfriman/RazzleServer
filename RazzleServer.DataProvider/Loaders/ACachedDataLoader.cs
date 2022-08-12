using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RazzleServer.Common;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public abstract class ACachedDataLoader<T> where T : new()
    {
        public abstract string CacheName { get; }

        public abstract ILogger Logger { get; }

        public T Data { get; private set; } = new T();

        public T Load()
        {
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");
            if (File.Exists(path))
            {
                try
                {
                    LoadFromCache();
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Error loading [{CacheName}] cache. Attempting to load from WZ. CachePath={path}");
                    EnsureWzFileReady();
                    LoadFromWz(CachedData.WzFile);
                    SaveToCache();
                }
            }
            else
            {
                Logger.Information($"[{CacheName}] cache not found. Attempting to load from WZ. CachePath={path}");
                EnsureWzFileReady();
                LoadFromWz(CachedData.WzFile);
                SaveToCache();
            }

            return Data;
        }

        private static void EnsureWzFileReady()
        {
            if (CachedData.WzFile != null)
            {
                return;
            }

            CachedData.WzFile = GetWzFile("Data.wz");
            CachedData.WzFile.ParseWzFile();
        }

        public void SaveToCache()
        {
            Directory.CreateDirectory(ServerConfig.Instance.CacheFolder);
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");

            File.WriteAllText(path, JsonSerializer.Serialize<object>(Data, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = ServerConfig.Instance.PrettifyCache
            }));

            Logger.Information($"Saving [{CacheName}] to cached file");
        }

        public void LoadFromCache()
        {
            var path = Path.Combine(ServerConfig.Instance.CacheFolder, $"{CacheName}.cache");
            Data = JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            Logger.Information($"Loaded [{CacheName}] from cache");
        }

        public abstract void LoadFromWz(WzFile file);

        public static WzFile GetWzFile(string name) => new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, name),
            (short)ServerConfig.Instance.Version, WzMapleVersionType.Classic);
    }
}
