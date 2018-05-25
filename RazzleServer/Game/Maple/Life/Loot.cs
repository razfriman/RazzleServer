namespace RazzleServer.Game.Maple.Life
{
    public sealed class Loot
    {
        public int MapleId { get; private set; }
        public int MinimumQuantity { get; private set; }
        public int MaximumQuantity { get; private set; }
        public int QuestId { get; private set; }
        public int Chance { get; private set; }
        public bool IsMeso { get; private set; }

        //public Loot(Datum datum)
        //{
        //    this.MapleId = (int)datum["itemid"];
        //    this.MinimumQuantity = (int)datum["minimum_quantity"];
        //    this.MaximumQuantity = (int)datum["maximum_quantity"];
        //    this.QuestId = (int)datum["questid"];
        //    this.Chance = (int)datum["chance"];
        //    this.IsMeso = ((string)datum["flags"]).Contains("is_mesos");
        //}
    }
}
