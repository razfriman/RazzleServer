using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DropPickup)]
    public class DropPickupHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.Skip(1);
            packet.Skip(4);
            var position = packet.ReadPoint();

            // TODO: Validate position relative to the picker.

            var objectId = packet.ReadInt();

            lock (client.Character.Map.Drops)
            {
                if (client.Character.Map.Drops.Contains(objectId))
                {
                    client.Character.Items.Pickup(client.Character.Map.Drops[objectId]);
                }
            }
        }
    }
}
