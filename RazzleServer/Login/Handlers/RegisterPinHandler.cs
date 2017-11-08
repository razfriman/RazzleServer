using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.RegisterPin)]
    public class RegisterPinHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var proceed = packet.ReadBool();

            if (proceed)
            {
                var pin = packet.ReadString();
                client.Account.Pin = Functions.GetSha512(pin);
                client.Account.Save();

                using (var oPacket = new PacketWriter(ServerOperationCode.PinCodeOperation))
                {
                    oPacket.WriteByte((byte)PinResult.Valid);
                    client.Send(oPacket);
                }
            }
        }
    }
}