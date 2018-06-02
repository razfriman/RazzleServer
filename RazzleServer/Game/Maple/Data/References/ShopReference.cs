using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data.References
{
    public class ShopReference
    {
        public int ShopId { get; set; }
        public int NpcId { get; set; }
        public int RechargeTier { get; set; }
        public List<ShopItemReference> ShopItems { get; set; } = new List<ShopItemReference>();
    }
}
