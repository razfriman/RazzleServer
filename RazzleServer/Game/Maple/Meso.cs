using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple
{
    public sealed class Meso : Drop
    {
        public int Amount { get; private set; }

        public Meso(int amount)
             : base()
        {
            this.Amount = amount;
        }

        public override PacketWriter GetShowGainPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.Message);

            oPacket.WriteByte((byte)MessageType.DropPickup);
            oPacket.WriteBool(true);
            oPacket.WriteByte(0); // NOTE: Unknown.
            oPacket.WriteInt(this.Amount);
            oPacket.WriteShort(0);

            return oPacket;
        }
    }
}
