using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedSkills
    {
        public Dictionary<int, Dictionary<byte, SkillReference>> Data { get; private set; } =
            new Dictionary<int, Dictionary<byte, SkillReference>>();
    }
}
