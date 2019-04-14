using System.Collections.Generic;
using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class MobSkillsLoader : ACachedDataLoader<CachedMobSkills>
    {
        public override string CacheName => "MobSkills";

        public override ILogger Logger => Log.ForContext<MobSkillsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading MobSkills");

            using var file = GetWzFile("Data.wz");
            file.ParseWzFile();
            var dir = file.WzDirectory.GetDirectoryByName("Skill");

            foreach (var skillImg in dir.GetImageByName("MobSkill.img").WzProperties)
            {
                if (!int.TryParse(skillImg.Name, out var id))
                {
                    continue;
                }

                Data.Data[id] = new Dictionary<byte, MobSkillDataReference>();

                foreach (var levelImg in skillImg["level"].WzProperties)
                {
                    if (!byte.TryParse(levelImg.Name, out var level))
                    {
                        continue;
                    }

                    Data.Data[id][level] = new MobSkillDataReference(levelImg);
                }
            }
        }
    }
}
