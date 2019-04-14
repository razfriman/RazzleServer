using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DropPickup)]
    public class DropPickupHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var position = packet.ReadPoint();

            // TODO: Validate position relative to the picker.

            var objectId = packet.ReadInt();

            lock (client.GameCharacter.Map.Drops)
            {
                if (client.GameCharacter.Map.Drops.Contains(objectId))
                {
                    client.GameCharacter.Items.Pickup(client.GameCharacter.Map.Drops[objectId]);
                }
            }
        }
    }
}
