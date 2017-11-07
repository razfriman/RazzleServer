using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MultiChat)]
    public class MultiChatHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {

        }
    }
}