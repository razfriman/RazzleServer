using RazzleServer.DataProvider.Cache;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class QuestsLoader : ACachedDataLoader<CachedQuests>
    {
        public override string CacheName => "Quests";

        public override ILogger Logger => Log.ForContext<QuestsLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Quests");
        }
    }
}
