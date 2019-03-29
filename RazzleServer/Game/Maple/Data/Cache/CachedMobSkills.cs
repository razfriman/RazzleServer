using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedMobSkills
    {
        public readonly Dictionary<int, Dictionary<byte, MobSkillDataReference>> Data =
            new Dictionary<int, Dictionary<byte, MobSkillDataReference>>();
    }
}
