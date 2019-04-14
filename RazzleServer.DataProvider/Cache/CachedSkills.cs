using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedSkills
    {
        public readonly Dictionary<int, Dictionary<byte, SkillReference>> Data =
            new Dictionary<int, Dictionary<byte, SkillReference>>();
    }
}
