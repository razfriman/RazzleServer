using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.WzLib;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedSkills : Dictionary<int, Dictionary<byte, Skill>>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedSkills()
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

                        this[id] = new Dictionary<byte, Skill>();

                        foreach (var levelImg in skillImg["level"].WzProperties)
                        {
                            if (!byte.TryParse(levelImg.Name, out var level))
                            {
                                continue;
                            }

                            this[id][level] = new Skill(levelImg);
                        }
                    }
                }
            }
        }
    }
}