using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class ReactorsLoader : ACachedDataLoader<CachedReactors>
    {
        public override string CacheName => "Reactors";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Reactors");

            using (var file = GetWzFile("Reactor.wz"))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x =>
                {
                    var reactor = new ReactorReference(x);
                    Data.Data.Add(reactor.MapleId, reactor);
                });
            }
        }
    }
}