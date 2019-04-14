using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcChat)]
    public class NpcResultHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            client.GameCharacter.NpcScript?.Npc.Handle(client.GameCharacter, packet);
        }
    }
}
