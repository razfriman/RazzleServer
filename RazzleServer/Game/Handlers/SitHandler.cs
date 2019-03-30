using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.FieldPlayerSitMapChair)]
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

            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSitOnChair))
            {
                oPacket.WriteBool(hasSeat);

                if (hasSeat)
                {
                    oPacket.WriteShort(seatId);
                }

                client.Character.Map.Send(oPacket, client.Character);
            }

            client.Character.Release();
        }
    }
}
