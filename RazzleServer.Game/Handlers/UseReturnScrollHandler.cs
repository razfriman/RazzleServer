using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseReturnScroll)]
    public class UseReturnScrollHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.GameCharacter.Items[itemId, slot];

            if (item == null)
            {
                return;
            }

            client.GameCharacter.Items.Remove(itemId, 1);
            client.GameCharacter.ChangeMap(item.CMoveTo);
        }
    }
}
