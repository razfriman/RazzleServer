using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.CharacterCreationRequest)]
    public class CharacterCreationRequestHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            byte worldID = packet.ReadByte();
            int accountID = packet.ReadInt();
            byte[] characterData = packet.ReadBytes((int)packet.Available);

            var pw = new PacketWriter(InteroperabilityOperationCode.CharacterCreationRequest);
            pw.WriteInt(accountID);
            pw.WriteBytes(characterData);
            client.Server.Worlds[worldID].RandomChannel.Send(pw);
        }
    }
}