using RazzleServer.Packet;
using System.Drawing;

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
