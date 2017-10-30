using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MOVE_PLAYER)]
    public class PlayerMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader pr, GameClient c)
        {
            
        }
    }
}