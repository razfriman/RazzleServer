using RazzleServer.Data;
using RazzleServer.Common.Packet;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Server;
using System.Security.Cryptography;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.DELETE_CHAR)]
    public class DeleteCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pic = packet.ReadString();
            var characterID = packet.ReadInt();


            CharacterDeletionResult result;


            if (!ServerConfig.Instance.RequestPic || Functions.GetSha1(pic) == client.Account.Pic)
            {
                //NOTE: As long as foreign keys are set to cascade, all child entries related to this CharacterID will also be deleted.
                Database.Delete("characters", "ID = {0}", characterID);

                result = CharacterDeletionResult.Valid;
            }
            else
            {
                result = CharacterDeletionResult.InvalidPic;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.DeleteCharacterResult))
            {
                oPacket.WriteInt(characterID);
                oPacket.WriteByte((byte)result);

                client.Send(oPacket);
            }
        }
    }
}
}