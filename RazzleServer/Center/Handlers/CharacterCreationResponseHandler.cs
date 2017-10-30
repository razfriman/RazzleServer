using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.CharacterCreationResponse)]
    public class CharacterCreationResponseHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            int accountID = packet.ReadInt();
            byte[] characterData = packet.ReadBytes((int)packet.Available);

            var pw = new PacketWriter(InteroperabilityOperationCode.CharacterCreationResponse);
            pw.WriteInt(accountID);
            pw.WriteBytes(characterData);
            client.Server.Login.Send(pw);
        }
    }
}