using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.FaceExpression)]
    public class FaceExpressionHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var expressionId = packet.ReadInt();
            client.Character.PerformFacialExpression(expressionId);
        }
    }
}
