using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcConverse)]
    public class NpcConverseHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();
            var playerPosition = packet.ReadPoint();
            client.Character.Converse(client.Character.Map.Npcs[objectId]);
        }
    }
}