using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedMaps
    {
        public Dictionary<int, MapReference> Data { get; set; } = new Dictionary<int, MapReference>();
    }
}
