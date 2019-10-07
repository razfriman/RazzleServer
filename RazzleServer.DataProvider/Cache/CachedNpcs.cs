using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CachedNpcs
    {
        public Dictionary<int, NpcReference> Data { get; set; } = new Dictionary<int, NpcReference>();
    }
}
