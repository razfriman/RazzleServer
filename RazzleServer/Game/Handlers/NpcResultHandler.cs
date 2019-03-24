using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcChat)]
    public class NpcResultHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            client.Character.NpcScript?.Npc.Handle(client.Character, packet);
        }
    }
}
