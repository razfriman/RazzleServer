using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class MobsLoader : ACachedDataLoader<CachedMobs>
    {
        public override string CacheName => "Mobs";

        public override ILogger Logger => Log.ForContext<MobsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Mobs");

            using var file = GetWzFile("Data.wz");
            file.ParseWzFile();
            var dir = file.WzDirectory.GetDirectoryByName("Mob");
            dir.WzImages.ForEach(x =>
            {
                var link = x["info"]["link"]?.GetString();
                    
                var mob = new MobReference(x, link != null ? dir.GetImageByName($"{link}.img") : null);
                Data.Data.Add(mob.MapleId, mob);
            });
        }
    }
}
