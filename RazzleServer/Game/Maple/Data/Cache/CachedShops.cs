using System.Collections.Generic;
using RazzleServer.Game.Maple.Shops;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public class CachedShops
    {
        public readonly Dictionary<int, Shop> Data = new Dictionary<int, Shop>();
    }
}
