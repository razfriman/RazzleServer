using RazzleServer.Packet;
using System.Drawing;

namespace RazzleServer.Movement
{
    class TeleportMovement : MapleMovementFragment
    {       

        public TeleportMovement(byte type, Point position, byte state, short duration, short fh)
            : base(type, position, state, duration)
        {       
            Foothold = fh;       
        }

        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type);
            pw.WritePoint(Position);
            pw.WriteShort(Foothold);         
            pw.WriteByte(State);
            pw.WriteShort(Duration);
        }
    }
}
