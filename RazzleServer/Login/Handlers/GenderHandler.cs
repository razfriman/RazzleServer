using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.AccountGender)]
    public class GenderHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            if (client.Account.Gender != Gender.Unset)
            {
                return;
            }

            var valid = packet.ReadBool();

            if (valid)
            {
                var gender = (Gender)packet.ReadByte();
                client.Account.Gender = gender;
                client.Account.Save();
                client.Send(LoginPackets.SendLoginResult(LoginResult.Valid, client.Account));
            }
        }
    }
}

