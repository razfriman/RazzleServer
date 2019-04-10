namespace RazzleServer.Game.Maple.Buffs
{
    public struct PrimaryStatsAddition
    {
        public int ItemID { get; set; }
        public short Slot { get; set; }
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Int { get; set; }
        public short Luk { get; set; }
        public short MaxHP { get; set; }
        public short MaxMP { get; set; }
        public short Speed { get; set; }
    }
}