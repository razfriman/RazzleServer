using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public abstract class LifeObject : MapObject
    {
        public int MapleID { get; private set; }
        public short Foothold { get; private set; }
        public short MinimumClickX { get; private set; }
        public short MaximumClickX { get; private set; }
        public bool FacesLeft { get; private set; }
        public int RespawnTime { get; private set; }
        public LifeObjectType Type { get; private set; }

        public LifeObject(WzImageProperty img, LifeObjectType type)
        {
            MapleID = int.Parse(img["id"].GetString());
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