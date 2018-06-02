namespace RazzleServer.Data
{
    public class ShopItemEntity
    {
        public int Id { get; set; }

        public int ShopId { get; set; }

        public int ItemId { get; set; }

        public short Quantity { get; set; }

        public int Price { get; set; }

        public int Sort { get; set; }

        public ShopEntity Shop { get; set; }
    }
}