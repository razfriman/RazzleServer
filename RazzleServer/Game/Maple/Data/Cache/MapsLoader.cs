using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class MapsLoader : ACachedDataLoader<CachedMaps>
    {
        public override string CacheName => "Maps";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
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
                    .ForEach(img =>
                    {
                        var map = new Map(img);
                        Data.Data.Add(map.MapleId, map);
                    });
            }
        }
    }
}