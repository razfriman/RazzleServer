using RazzleServer.Common.Constants;
using RazzleServer.Login.Maple;
using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.DeleteCharacter)]
    public class DeleteCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            packet.ReadInt(); // Birthday
            var characterId = packet.ReadInt();
            LoginCharacter.Delete(client.Account.Id, characterId);

            using var pw = new PacketWriter(ServerOperationCode.DeleteCharacterResult);
            pw.WriteInt(characterId);
            pw.WriteByte(CharacterDeletionResult.Valid);
            client.Send(pw);
        }
    }
}
