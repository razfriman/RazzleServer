using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedMobs
    {
        public Dictionary<int, MobReference> Data { get; set; } = new Dictionary<int, MobReference>();
    }
}
