using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.Sit)]
    public class SitHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var seatId = packet.ReadShort();
            var hasSeat = seatId != -1;

            if (hasSeat)
            {
                client.Character.Chair = seatId;
            }
            else
            {
                RemoveChair(client);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Sit))
            {
                oPacket.WriteBool(hasSeat);

                if (hasSeat)
                {
                    oPacket.WriteShort(seatId);
                }

                client.Send(oPacket);
            }
        }

        private static void RemoveChair(GameClient client)
        {
            client.Character.Chair = 0;

            using (var oPacket = new PacketWriter(ServerOperationCode.ShowChair))
            {
                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteInt(0);
                client.Character.Map.Send(oPacket, client.Character);
            }
        }
    }
}