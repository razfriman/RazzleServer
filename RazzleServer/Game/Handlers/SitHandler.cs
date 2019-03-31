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

            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerSitOnChair))
            {
                pw.WriteBool(hasSeat);

                if (hasSeat)
                {
                    pw.WriteShort(seatId);
                }

                client.Character.Map.Send(pw, client.Character);
            }

            client.Character.Release();
        }
    }
}
