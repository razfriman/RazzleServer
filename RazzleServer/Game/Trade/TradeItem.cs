using RazzleServer.Inventory;

namespace RazzleServer.Player.Trade
{
    public class TradeItem
    {
        public MapleItem Item { get; private set; }
        public short Count { get; private set; }
        public TradeItem(MapleItem item, short count)
        {
            Count = count;
            Item = item;
        }
    }
}
