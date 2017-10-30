namespace RazzleServer.Data.WZ
{
    public class WzConsume : WzItem
    {
        public int Hp { get; set; }
        public int Mp { get; set; }
        public byte HpR { get; set; }
        public byte MpR { get; set; }
        public int Speed { get; set; }
        public int Time { get; set; }
        public int MoveTo { get; set; }

        public int CraftExp { get; set; }
        public int CharmExp { get; set; }
        public int CharismaExp { get; set; }
        public int InsightExp { get; set; }
        public int WillExp { get; set; }
        public int SenseExp { get; set; }
    }
}
