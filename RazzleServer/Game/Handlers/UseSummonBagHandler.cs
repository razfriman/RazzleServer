using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSummonBag)]
    public class UseSummonBagHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {

        }
    }
}