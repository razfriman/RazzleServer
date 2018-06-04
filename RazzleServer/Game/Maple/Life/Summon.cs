using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Util;

namespace RazzleServer.Game.Maple.Life
{
    public class Summon : MapObject, ISpawnable
    {
        public PacketWriter GetCreatePacket()
        {
            throw new System.NotImplementedException();
        }

        public PacketWriter GetDestroyPacket()
        {
            throw new System.NotImplementedException();
        }

        public PacketWriter GetSpawnPacket()
        {
            throw new System.NotImplementedException();
        }
    }
}
