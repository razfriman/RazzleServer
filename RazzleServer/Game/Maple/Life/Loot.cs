namespace RazzleServer.Game.Maple.Life
{
    public sealed class Loot
    {
        public int MobId { get; set; }
        public int ItemId { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public int QuestId { get; set; }
        public int Chance { get; set; }
        public bool IsMeso { get; set; }
    }
}
