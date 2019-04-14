using RazzleServer.DataProvider.Cache;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class QuestsLoader : ACachedDataLoader<CachedQuests>
    {
        public override string CacheName => "Quests";

        public override ILogger Logger => Log.ForContext<QuestsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Quests");
        }
    }
}
