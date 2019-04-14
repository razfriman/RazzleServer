using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DistributeAp)]
    public class DistributeApHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (StatisticType)packet.ReadInt();
            client.GameCharacter.PrimaryStats.DistributeAp(type);
        }
    }
}
