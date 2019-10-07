using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedMobSkills
    {
        public readonly Dictionary<int, Dictionary<byte, MobSkillDataReference>> Data =
            new Dictionary<int, Dictionary<byte, MobSkillDataReference>>();
    }
}
