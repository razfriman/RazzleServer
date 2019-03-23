using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.ClientCrashReport)]
    public class ClientCrashReportHandler : LoginPacketHandler
    {
        public static readonly ILogger Log = LogManager.CreateLogger<ClientCrashReportHandler>();
        
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var message = packet.ReadString();
            Log.LogWarning($"Client Crashed Host={client.Host} Message={message}");
        }
    }
}
