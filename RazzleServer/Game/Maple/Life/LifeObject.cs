using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public abstract class LifeObject : MapObject
    {
        public int MapleId { get; }
        public short Foothold { get; }
        public short MinimumClickX { get; }
        public short MaximumClickX { get; }
        public bool FacesLeft { get; }
        public int RespawnTime { get; }
        public LifeObjectType Type { get; private set; }

        protected LifeObject() { }

        protected LifeObject(WzImageProperty img, LifeObjectType type)
        {
            MapleId = int.Parse(img["id"].GetString());
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Foothold = img["fh"]?.GetShort() ?? 0;
            MinimumClickX = img["rx0"]?.GetShort() ?? 0;
            MaximumClickX = img["rx1"]?.GetShort() ?? 0;
            FacesLeft = (img["f"]?.GetInt() ?? 0) > 0;

            if (type == LifeObjectType.Mob)
            {
                RespawnTime = img["mobTime"]?.GetInt() ?? 0;
            }
            else if (type == LifeObjectType.Reactor)
            {
                RespawnTime = img["reactorTime"]?.GetInt() ?? 0;
            }
        }
    }
}