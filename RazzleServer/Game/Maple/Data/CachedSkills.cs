using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedSkills : Dictionary<int, Dictionary<byte, Skill>>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedSkills()
            : base()
        {
            Log.LogInformation("Loading Skills");
        }
    }
}
