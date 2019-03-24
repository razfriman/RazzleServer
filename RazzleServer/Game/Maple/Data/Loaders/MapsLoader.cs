using System.Linq;
using Serilog;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class MapsLoader : ACachedDataLoader<CachedMaps>
    {
        public override string CacheName => "Maps";

        public override ILogger Logger => Log.ForContext<MapsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Maps");

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
