using System.Drawing;
using MapleLib.PacketLib;

namespace RazzleServer.Movement
{
    class BounceMovement : MapleMovementFragment
    {
        public Point Offset { get; set; }

        public BounceMovement(byte type, Point position, byte state, short duration,Point offset)
            : base(type, position, state, duration)
        {           
            Offset = offset;           
        }

        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type);
            pw.WritePoint(Position);
            pw.WritePoint(Offset);
            pw.WriteByte(State);
            pw.WriteShort(Duration);
        }
    }
}
