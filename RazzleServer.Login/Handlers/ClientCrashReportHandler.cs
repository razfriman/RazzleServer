using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.ClientCrashReport)]
    public class ClientCrashReportHandler : LoginPacketHandler
    {
        private readonly ILogger _log = Log.ForContext<ClientCrashReportHandler>();

        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var message = packet.ReadString();
            _log.Warning($"Client Crashed Host={client.Host} Message={message}");
        }
    }
}
