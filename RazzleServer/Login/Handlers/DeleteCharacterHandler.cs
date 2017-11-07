using RazzleServer.Common.Packet;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterDelete)]
    public class DeleteCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var characterID = packet.ReadInt();
            var result = CharacterDeletionResult.Valid;

            Character.Delete(characterID);

            using (var oPacket = new PacketWriter(ServerOperationCode.DeleteCharacterResult))
            {
                oPacket.WriteInt(characterID);
                oPacket.WriteByte((byte)result);
                client.Send(oPacket);
            }
        }
    }
}
