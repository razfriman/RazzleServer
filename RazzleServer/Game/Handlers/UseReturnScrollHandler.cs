using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseReturnScroll)]
    public class UseReturnScrollHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt(); // NOTE: Ticks.
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.Character.Items[itemId, slot];

            if (item == null)
            {
                return;
            }

            client.Character.Items.Remove(itemId, 1);
            client.Character.ChangeMap(item.CMoveTo);
        }
    }
}