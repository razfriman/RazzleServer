using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class MapsLoader : ACachedDataLoader<CachedMaps>
    {
        public override string CacheName => "Maps";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Maps");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Map.wz"), WzMapleVersion.Classic))
            {
                file.ParseWzFile();
                file.WzDirectory
                    .GetDirectoryByName("Map")
                    .WzDirectories
                    .SelectMany(dir => dir.WzImages)
                    .ToList()
                    .ForEach(img =>
                    {
                    var map = new MapReference(img);
                        Data.Data.Add(map.MapleId, map);
                    });
            }
        }
    }
}