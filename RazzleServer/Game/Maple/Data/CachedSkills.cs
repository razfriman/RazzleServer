using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedSkills : Dictionary<int, Dictionary<byte, Skill>>
    {
        private static readonly ILogger Log = LogManager.Log;

        public CachedSkills()
            : base()
        {
            Log.LogInformation("Loading Skills");
            foreach (Datum datum in new Datums("skill_player_data").Populate())
            {
                this.Add((int)datum["skillid"], new Dictionary<byte, Skill>());
            }

            foreach (Datum datum in new Datums("skill_player_level_data").Populate())
            {
                this[(int)datum["skillid"]].Add((byte)(short)datum["skill_level"], new Skill(datum));
            }
        }
    }
}
