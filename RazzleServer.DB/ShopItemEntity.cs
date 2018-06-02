using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RazzleServer.Data
{
    public class ShopItemEntity
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Shop")]
        public int ShopId { get; set; }
        public int ItemId { get; set; }
        public short Quantity { get; set; }
        public int Price { get; set; }
        public int Sort { get; set; }
        public ShopEntity Shop { get; set; }
    }
}