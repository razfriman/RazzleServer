using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public abstract class LifeObject : IMapObject
    {
        public int MapleId { get; set; }
        public short Foothold { get; set; }
        public short MinimumClickX { get; set; }
        public short MaximumClickX { get; set; }
        public bool FacesLeft { get; set; }
        public int RespawnTime { get; set; }
        public bool Hide { get; set; }
        public LifeObjectType Type { get; set; }
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }

        protected LifeObject()
        {
        }
        
        protected LifeObject(MapNpcReference reference)
        {
            MapleId = reference.MapleId;
            Position = reference.Position;
            Foothold = reference.Foothold;
            MinimumClickX = reference.MinimumClickX;
            MaximumClickX = reference.MaximumClickX;
            FacesLeft = reference.FacesLeft;
            Hide = reference.Hide;
            Type = reference.Type;
            RespawnTime = reference.RespawnTime;
        }

        protected LifeObject(SpawnPointReference reference)
        {
            MapleId = reference.MapleId;
            Position = reference.Position;
            Foothold = reference.Foothold;
            MinimumClickX = reference.MinimumClickX;
            MaximumClickX = reference.MaximumClickX;
            FacesLeft = reference.FacesLeft;
            Hide = reference.Hide;
            Type = reference.Type;
            RespawnTime = reference.RespawnTime;
        }
    }
}
