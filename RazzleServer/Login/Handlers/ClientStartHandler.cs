using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using MapleLib.PacketLib;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.CLIENT_START)]
    public class ClientStartHandler : APacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var message = packet.ReadString();
            Log.LogInformation($"Client Start Message: {message}");
        }
    }
}