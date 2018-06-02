using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class MobSkillsLoader : ACachedDataLoader<CachedMobSkills>
    {
        public override string CacheName => "MobSkills";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading MobSkills");

            using (var file = GetWzFile("Skill.wz"))
            {
                file.ParseWzFile();


                foreach (var skillImg in file.WzDirectory.GetImageByName("MobSkill.img").WzProperties)
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
}