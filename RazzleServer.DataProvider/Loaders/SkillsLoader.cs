using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class SkillsLoader : ACachedDataLoader<CachedSkills>
    {
        public override string CacheName => "Skills";

        public override ILogger Logger => Log.ForContext<SkillsLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Skills");

            var dir = file.WzDirectory.GetDirectoryByName("Skill");

            foreach (var job in Enum.GetValues(typeof(Job)))
            {
                var imgName = $"{((short)job).ToString().PadLeft(3, '0')}.img";
                var img = dir.GetImageByName(imgName);

                if (img == null)
                {
                    continue;
                }

                foreach (var skillImg in img["skill"].WzPropertiesList)
                {
                    if (!int.TryParse(skillImg.Name, out var id))
                    {
                        continue;
                    }

                    Data.Data[id] = new Dictionary<byte, SkillReference>();

                    foreach (var levelImg in skillImg["level"].WzPropertiesList)
                    {
                        if (!byte.TryParse(levelImg.Name, out var level))
                        {
                            continue;
                        }

                        Data.Data[id][level] = new SkillReference(id, level, levelImg);
                    }
                }
            }
        }
    }
}
