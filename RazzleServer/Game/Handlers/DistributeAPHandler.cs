using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.StatsChange)]
    public class DistributeApHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (StatisticType)packet.ReadInt();
            client.Character.DistributeAp(type);
        }
    }
}
