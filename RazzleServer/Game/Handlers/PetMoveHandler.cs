using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PetMove)]
    public class PetMoveHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {

        }
    }
}
