namespace RazzleServer.Game.Maple.Life
{
    public sealed class Loot
    {
        public int MapleId { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public int QuestId { get; set; }
        public int Chance { get; set; }
        public bool IsMeso { get; set; }

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
