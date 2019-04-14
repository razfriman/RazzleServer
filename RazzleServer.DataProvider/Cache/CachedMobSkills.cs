using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedMobSkills
    {
        public readonly Dictionary<int, Dictionary<byte, MobSkillDataReference>> Data =
            new Dictionary<int, Dictionary<byte, MobSkillDataReference>>();
    }
}
