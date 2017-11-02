using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CLIENT_START)]
    public class ClientStartHandler : LoginPacketHandler
    {
        private ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var message = packet.ReadString();
            Log.LogInformation($"Client Start Message: {message}");
        }
    }
}