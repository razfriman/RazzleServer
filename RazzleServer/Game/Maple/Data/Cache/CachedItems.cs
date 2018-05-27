using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedItems
    {
        public List<int> WizetItemIds { get; private set; } = new List<int>
        {
            1002140,
            1322013,
            1042003,
            1062007
        };

        public Dictionary<int, ItemReference> Data { get; set; } = new Dictionary<int, ItemReference>();
    }
}