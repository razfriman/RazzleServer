using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.PinCheck)]
    public class CheckPinHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            byte a = packet.ReadByte();
            byte b = packet.ReadByte();

            PinResult result;

            if (b == 0)
            {
                string pin = packet.ReadString();

                if (Functions.GetSha512(pin) != client.Account.Pin)
                {
                    result = PinResult.Invalid;
                }
                else
                {
                    if (a == 1)
                    {
                        result = PinResult.Valid;
                    }
                    else if (a == 2)
                    {
                        result = PinResult.Register;
                    }
                    else
                    {
                        result = PinResult.Error;
                    }
                }
            }
            else if (b == 1)
            {
                if (string.IsNullOrEmpty(client.Account.Pin))
                {
                    result = PinResult.Register;
                }
                else
                {
                    result = PinResult.Request;
                }
            }
            else
            {
                result = PinResult.Error;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.CheckPinCodeResult))
            {
                oPacket.WriteByte((byte)result);
                client.Send(oPacket);
            }
        }
    }
}