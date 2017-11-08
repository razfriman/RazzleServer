using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.Sit)]
    public class SitHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            short seatID = packet.ReadShort();
            var isSitting = seatID == -1;
            if (!isSitting)
            {
                RemoveChair(client);
            }
            else
            {
                client.Character.Chair = seatID;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Sit))
            {
                oPacket.WriteBool(isSitting);

                if (isSitting)
                {
                    oPacket.WriteShort(seatID);
                }

                client.Send(oPacket);
            }
        }

        private static void RemoveChair(GameClient client)
        {
            client.Character.Chair = 0;

            using (var oPacket = new PacketWriter(ServerOperationCode.CancelChair))
            {
                oPacket.WriteInt(client.Character.ID);
                oPacket.WriteInt(0);
                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}