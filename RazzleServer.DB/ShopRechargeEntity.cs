using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class ShopRechargeEntity
    {
        [Key]
        public int Id { get; set; }
        public int TierId { get; set; }
        public int ItemId { get; set; }
        public double Price { get; set; }
    }
}
