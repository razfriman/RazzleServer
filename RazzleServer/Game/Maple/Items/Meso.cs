using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Items
{
    public sealed class Meso : Drop
    {
        public int Amount { get; }

        public Meso(int amount) => Amount = amount;

        public override PacketWriter GetShowGainPacket() =>
            GamePackets.ShowStatusInfo(MessageType.DropPickup, true, Amount);
    }
}
