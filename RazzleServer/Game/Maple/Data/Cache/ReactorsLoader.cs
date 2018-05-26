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
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class ReactorsLoader : ACachedDataLoader<CachedReactors>
    {
        public override string CacheName => "Reactors";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Reactors");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Reactor.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x =>
                {
                    var reactor = new Reactor(x);
                    Data.Data.Add(reactor.MapleId, reactor);
                });
            }
        }
    }
}