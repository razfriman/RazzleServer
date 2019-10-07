using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedMaps
    {
        public Dictionary<int, MapReference> Data { get; set; } = new Dictionary<int, MapReference>();
    }
}
