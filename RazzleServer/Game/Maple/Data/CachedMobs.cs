using System.Collections.ObjectModel;
using RazzleServer.Game.Maple.Life;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Server;
using RazzleServer.Common.WzLib;
using System.IO;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedMobs : KeyedCollection<int, Mob>
    {
        private readonly ILogger Log = LogManager.Log;

        public void Load()
        {
            Log.LogInformation("Loading Mobs");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Mob.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x => Add(new Mob(x)));
            }

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Skill.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();
                file.WzDirectory.GetImageByName("MobSkill.img");
                // TODO - Load Mob Skills
            }
        }

        protected override int GetKeyForItem(Mob item) => item.MapleId;
    }
}
