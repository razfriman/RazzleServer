using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class MobsLoader : ACachedDataLoader<CachedMobs>
    {
        public override string CacheName => "Mobs";

        public override ILogger Log => LogManager.CreateLogger<MobsLoader>();

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Mobs");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var dir = file.WzDirectory.GetDirectoryByName("Mob");
                dir.WzImages.ForEach(x =>
                {
                    var mob = new MobReference(x);
                    Data.Data.Add(mob.MapleId, mob);
                });
            }
        }
    }
}
