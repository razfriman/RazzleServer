using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedItems
    {
        public List<int> WizetItemIds { get; } = new List<int> {1002140, 1322013, 1042003, 1062007};

        public Dictionary<int, ItemReference> Data { get; set; } = new Dictionary<int, ItemReference>();
    }
}
