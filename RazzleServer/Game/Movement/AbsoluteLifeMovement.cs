using System.Drawing;
using MapleLib.PacketLib;

namespace RazzleServer.Movement
{
    class AbsoluteLifeMovement : MapleMovementFragment
    {
        public Point Wobble { get; set; }
        public Point Offset { get; set; }
        public short FhFallStart { get; set; }

        public AbsoluteLifeMovement(byte type, Point position, byte state, short duration, Point wobble, Point offset, short fh, short fhFallStart)
            : base(type, position, state, duration)
        {
            Wobble = wobble;
            Offset = offset;
            Foothold = fh;
            FhFallStart = fhFallStart;
        }
              
        public override void Serialize(PacketWriter pw) 
        {
            pw.WriteByte(Type);
            pw.WritePoint(Position);
            pw.WritePoint(Wobble);
            pw.WriteShort(Foothold);
            if (Type == 15)
                pw.WriteShort(FhFallStart);
            if (Type != 58)
                pw.WritePoint(Offset);
            pw.WriteByte(State);
            pw.WriteShort(Duration);            
        }
    }
}
