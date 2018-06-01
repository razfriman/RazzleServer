using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple
{
    public sealed class Meso : Drop
    {
        public int Amount { get; }

        public Meso(int amount)
        {
            Amount = amount;
        }

        public override PacketWriter GetShowGainPacket()
        {
            return GamePackets.ShowStatusInfo(MessageType.DropPickup, true, Amount);
        }
    }
}
