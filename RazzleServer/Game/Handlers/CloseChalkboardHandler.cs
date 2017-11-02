using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Commands;
using RazzleServer.Server;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CloseChalkboard)]
    public class CloseChalkboardHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            client.Character.Chalkboard = string.Empty;
        }
    }
}