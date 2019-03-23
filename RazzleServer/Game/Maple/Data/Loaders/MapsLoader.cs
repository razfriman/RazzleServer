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

        public override ILogger Log => LogManager.CreateLogger<MapsLoader>();

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Maps");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                file.WzDirectory.GetDirectoryByName("Map")
                    .GetDirectoryByName("Map")
                    .WzDirectories
                    .SelectMany(subDir => subDir.WzImages)
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
