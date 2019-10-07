using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedMobs
    {
        public Dictionary<int, MobReference> Data { get; set; } = new Dictionary<int, MobReference>();
    }
}
