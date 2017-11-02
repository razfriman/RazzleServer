using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.ClientStart)]
    public class ClientStartHandler : LoginPacketHandler
    {
        private readonly ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var message = packet.ReadString();
            Log.LogInformation($"Client Start Message: {message}");
        }
    }
}