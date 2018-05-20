using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.Sit)]
    public class SitHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            short seatId = packet.ReadShort();
            var isSitting = seatId == -1;
            if (!isSitting)
            {
                RemoveChair(client);
            }
            else
            {
                client.Character.Chair = seatId;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Sit))
            {
                oPacket.WriteBool(isSitting);

                if (isSitting)
                {
                    oPacket.WriteShort(seatId);
                }

                client.Send(oPacket);
            }
        }

        private static void RemoveChair(GameClient client)
        {
            client.Character.Chair = 0;

            using (var oPacket = new PacketWriter(ServerOperationCode.CancelChair))
            {
                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteInt(0);
                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}