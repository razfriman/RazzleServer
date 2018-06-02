using System.Collections.Generic;

namespace RazzleServer.Data
{
    public class ShopEntity
    {
        public int Id { get; set; }

        public int NpcId { get; set; }

        public byte RechargeTier { get; set; }

        public ICollection<ShopItemEntity> ShopItems { get; set; } = new List<ShopItemEntity>();
    }
}