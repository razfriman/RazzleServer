using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedMaps : KeyedCollection<int, Map>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedMaps()
        {
            Log.LogInformation("Loading Maps");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Map.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();
                file.WzDirectory
                    .GetDirectoryByName("Map")
                    .WzDirectories
                    .SelectMany(dir => dir.WzImages)
                    .ToList()
                    .ForEach(img => Add(new Map(img)));
            }
        }

        protected override int GetKeyForItem(Map item) => item.MapleID;
    }
}
