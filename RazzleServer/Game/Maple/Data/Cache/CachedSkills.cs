using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedSkills
    {
        public readonly Dictionary<int, Dictionary<byte, SkillReference>> Data =
            new Dictionary<int, Dictionary<byte, SkillReference>>();
    }
}
