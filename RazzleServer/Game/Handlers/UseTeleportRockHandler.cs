using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseTeleportRock)]
    public class UseTeleportRockHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {

        }
    }
}