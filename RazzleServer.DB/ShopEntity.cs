using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class ShopEntity
    {
        [Key]
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int NpcId { get; set; }
        public byte RechargeTier { get; set; }
        public ICollection<ShopItemEntity> ShopItems { get; set; } = new List<ShopItemEntity>();
    }
}