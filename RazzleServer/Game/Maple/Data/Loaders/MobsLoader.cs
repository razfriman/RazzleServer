using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Server;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class MobsLoader : ACachedDataLoader<CachedMobs>
    {
        public override string CacheName => "Mobs";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Mobs");

            using (var file = GetWzFile("Mob.wz"))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x =>
                {
                    var mob = new MobReference(x);
                    Data.Data.Add(mob.MapleId, mob);
                });
            }

            using (var file = GetWzFile("Skill.wz"))
            {
                file.ParseWzFile();
                file.WzDirectory.GetImageByName("MobSkill.img");
                // TODO - Load Mob Skills
            }
        }
    }
}