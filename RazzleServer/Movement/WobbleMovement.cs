using RazzleServer.Packet;
using System.Drawing;

namespace RazzleServer.Movement
{
    class WobbleMovement : MapleMovementFragment
    {
        private readonly short FhFallStart;
        private readonly Point Wobble;

        public WobbleMovement(byte type, Point wobble, short fhfallStart, byte state, short duration)
            : base(type, new Point(), state, duration )
        {
            FhFallStart = fhfallStart;
            Wobble = wobble;
        }

        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type);
            pw.WritePoint(Wobble);
            pw.WriteShort(FhFallStart);
            pw.WriteByte(State);
            pw.WriteShort(Duration);
        }
    }
}
