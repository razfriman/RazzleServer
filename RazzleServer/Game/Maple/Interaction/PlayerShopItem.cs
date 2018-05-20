namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class PlayerShopItem : Item
    {
        public short Bundles { get; set; }
        public int MerchantPrice { get; private set; }

        public PlayerShopItem(int mapleId, short bundles, short quantity, int price)
            : base(mapleId, quantity)
        {
            Bundles = bundles;
            MerchantPrice = price;
        }
    }
}
