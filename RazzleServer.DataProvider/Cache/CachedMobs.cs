using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedMobs
    {
        public Dictionary<int, MobReference> Data { get; set; } = new Dictionary<int, MobReference>();
    }
}
