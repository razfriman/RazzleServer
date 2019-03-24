using RazzleServer.Common.Packet;
using Serilog;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.ClientCrashReport)]
    public class ClientCrashReportHandler : LoginPacketHandler
    {
        public static readonly ILogger Log = Log.ForContext<ClientCrashReportHandler>();
        
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var message = packet.ReadString();
            Log.Warning($"Client Crashed Host={client.Host} Message={message}");
        }
    }
}
