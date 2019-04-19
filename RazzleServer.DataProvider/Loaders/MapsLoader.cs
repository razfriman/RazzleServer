using System.Linq;
using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class MapsLoader : ACachedDataLoader<CachedMaps>
    {
        public override string CacheName => "Maps";

        public override ILogger Logger => Log.ForContext<MapsLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Maps");

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
