using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.FaceExpression)]
    public class FaceExpressionHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int expressionID = packet.ReadInt();
            client.Character.PerformFacialExpression(expressionID);
        }
    }
}