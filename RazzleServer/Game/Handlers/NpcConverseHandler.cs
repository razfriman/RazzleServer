using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcConverse)]
    public class NpcConverseHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectID = packet.ReadInt();
            client.Character.Converse(client.Character.Map.Npcs[objectID]);
        }
    }
}