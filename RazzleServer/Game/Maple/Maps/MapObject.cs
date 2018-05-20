namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObject
    {
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
    }
}
