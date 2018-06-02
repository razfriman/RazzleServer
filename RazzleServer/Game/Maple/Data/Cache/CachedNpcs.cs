using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public class CachedNpcs
    {
        public Dictionary<int, NpcReference> Data { get; set; } = new Dictionary<int, NpcReference>();
    }
}
