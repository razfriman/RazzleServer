using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
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

            using (var file = GetWzFile("Map.wz"))
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