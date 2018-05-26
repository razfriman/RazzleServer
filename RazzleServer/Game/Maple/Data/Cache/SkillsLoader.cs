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

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class SkillsLoader : ACachedDataLoader<CachedSkills>
    {
        public override string CacheName => "Skills";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Skills");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Skill.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();

                foreach (var job in Enum.GetValues(typeof(Job)))
                {
                    var imgName = $"{((short)job).ToString().PadLeft(3, '0')}.img";
                    var img = file.WzDirectory.GetImageByName(imgName);

                    foreach (var skillImg in img["skill"].WzProperties)
                    {
                        if (!int.TryParse(skillImg.Name, out var id))
                        {
                            continue;
                        }

                        Data.Data[id] = new Dictionary<byte, Skill>();

                        foreach (var levelImg in skillImg["level"].WzProperties)
                        {
                            if (!byte.TryParse(levelImg.Name, out var level))
                            {
                                continue;
                            }

                            Data.Data[id][level] = new Skill(levelImg);
                        }
                    }
                }
            }
        }
    }
}