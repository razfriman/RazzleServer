using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RazzleServer.Data
{
    public class ShopEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShopId { get; set; }

        public int NpcId { get; set; }
        public byte RechargeTier { get; set; }
        public ICollection<ShopItemEntity> ShopItems { get; set; } = new List<ShopItemEntity>();
    }
}
