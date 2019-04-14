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
                client.GameCharacter.Chair = seatId;
            }

            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerSitOnChair);
            pw.WriteBool(hasSeat);
            if (hasSeat)
            {
                pw.WriteShort(seatId);
            }

            client.GameCharacter.Map.Send(pw, client.GameCharacter);
            client.GameCharacter.Release();
        }
    }
}
