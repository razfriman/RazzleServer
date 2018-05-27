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
    public sealed class MobsLoader : ACachedDataLoader<CachedMobs>
    {
        public override string CacheName => "Mobs";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Mobs");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Mob.wz"), WzMapleVersion.Classic))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x =>
                {
                    var mob = new MobReference(x);
                    Data.Data.Add(mob.MapleId, mob);
                });
            }

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Skill.wz"), WzMapleVersion.Classic))
            {
                file.ParseWzFile();
                file.WzDirectory.GetImageByName("MobSkill.img");
                // TODO - Load Mob Skills
            }
        }
    }
}