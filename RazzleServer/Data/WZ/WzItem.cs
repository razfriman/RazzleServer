namespace RazzleServer.Data.WZ
{
    public class WzItem
    {
        public int ItemId { get; set; }
        public bool Only { get; set; } //only one allowed in inventory
        public bool NotSale { get; set; }
        public bool IsCashItem { get; set; }
        public int SlotMax { get; set; }
        public int Price { get; set; }
        //public int MobId { get; set; }
        public bool TradeBlock { get; set; }
        public bool AccountShareable { get; set; }
        public bool IsQuestItem { get; set; }
        public string Name { get; set; }
        public bool Tradeable => !IsQuestItem && !TradeBlock && !AccountShareable && !IsCashItem;
    }
}
