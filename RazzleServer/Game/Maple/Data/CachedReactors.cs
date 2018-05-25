using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedReactors : KeyedCollection<int, Reactor>
    {
        private readonly ILogger Log = LogManager.Log;

        public void Load()
        {
            Log.LogInformation("Loading Reactors");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Reactor.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x => Add(new Reactor(x)));
            }
        }

        protected override int GetKeyForItem(Reactor item) => item.MapleId;
    }
}
