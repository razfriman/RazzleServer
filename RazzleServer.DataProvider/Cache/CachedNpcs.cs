using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public class CachedNpcs
    {
        public Dictionary<int, NpcReference> Data { get; set; } = new Dictionary<int, NpcReference>();
    }
}
