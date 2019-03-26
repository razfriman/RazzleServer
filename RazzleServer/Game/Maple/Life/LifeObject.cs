using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Life
{
    public abstract class LifeObject : MapObject
    {
        public int MapleId { get; set; }
        public short Foothold { get; set; }
        public short MinimumClickX { get; set; }
        public short MaximumClickX { get; set; }
        public bool FacesLeft { get; set; }
        public int RespawnTime { get; set; }
        public bool Hide { get; set; }
        public LifeObjectType Type { get; set; }

        protected LifeObject() { }

        protected LifeObject(WzImageProperty img, LifeObjectType type)
        {
            MapleId = int.Parse(img["id"].GetString());
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Foothold = img["fh"]?.GetShort() ?? 0;
            MinimumClickX = img["rx0"]?.GetShort() ?? 0;
            MaximumClickX = img["rx1"]?.GetShort() ?? 0;
            FacesLeft = (img["f"]?.GetInt() ?? 0) > 0;
            Hide = (img["hide"]?.GetInt() ?? 0) > 0;
            Type = type;

            if (type == LifeObjectType.Mob)
            {
                RespawnTime = img["mobTime"]?.GetInt() ?? 0;
            }
        }
    }
}
