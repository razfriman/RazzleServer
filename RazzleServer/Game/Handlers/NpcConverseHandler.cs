using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcSelect)]
    public class NpcConverseHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();
            client.Character.Converse(client.Character.Map.Npcs[objectId]);
        }
    }
}
