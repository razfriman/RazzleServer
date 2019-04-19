using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Wz;

namespace RazzleServer.DataProvider.References
{
    public class SpawnPointReference : LifeObjectReference
    {
        public SpawnPointReference()
        {
        }

        public SpawnPointReference(WzImageProperty img, LifeObjectType type) : base(img, type)
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
