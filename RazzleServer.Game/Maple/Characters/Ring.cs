namespace RazzleServer.Game.Maple.Characters
{
    public class Ring
    {
        public int MapleId { get; set; }
        public int PartnerRingId { get; set; }

        public Character Partner { get; set; }
    }
}
