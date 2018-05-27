using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedMaps
    {
        public Dictionary<int, MapReference> Data { get; set; } = new Dictionary<int, MapReference>();
    }
}
