using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.StrangeData)]
    public class StrangeDataHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)  { }
    }
}