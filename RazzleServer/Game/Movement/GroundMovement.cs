using RazzleServer.Common.Packet;
using System.Drawing;
using MapleLib.PacketLib;

namespace RazzleServer.Movement
{
    class GroundMovement : MapleMovementFragment
    {
        public GroundMovement(byte type, Point position, byte state, short duration)
            : base(type, position, state, duration) { }
             
        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type); 
            pw.WriteByte(State);
            pw.WriteShort(Duration);
        }
    }
}
