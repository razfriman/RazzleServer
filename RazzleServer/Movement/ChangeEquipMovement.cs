using RazzleServer.Packet;

namespace RazzleServer.Movement
{
    class ChangeEquipMovement : MapleMovementFragment
    {
        private byte wui;

        public ChangeEquipMovement(byte type, byte wui)
            : base(type, new Point(0, 0), 0, 0)
        {
            this.wui = wui;
        }

        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type);
            pw.WriteByte(wui);
        }
    }
}
