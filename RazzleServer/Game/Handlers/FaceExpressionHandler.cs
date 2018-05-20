using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.FaceExpression)]
    public class FaceExpressionHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int expressionId = packet.ReadInt();
            client.Character.PerformFacialExpression(expressionId);
        }
    }
}