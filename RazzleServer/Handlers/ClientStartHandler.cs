using Microsoft.Extensions.Logging;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using RazzleServer.Packet;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CLIENT_START)]
    public class ClientStartHandler : APacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var message = packet.ReadMapleString();
            Log.LogInformation($"Client Start Message: {message}");
        }
    }
}