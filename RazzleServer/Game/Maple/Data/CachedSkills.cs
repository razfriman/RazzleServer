using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedSkills
    {
        public Dictionary<int, Dictionary<byte, Skill>> Data = new Dictionary<int, Dictionary<byte, Skill>>();
    }
}