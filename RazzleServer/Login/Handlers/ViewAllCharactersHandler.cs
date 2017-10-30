using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.VIEW_ALL_CHAR)]
    public class ViewAllCharactersHandler : LoginPacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            Log.LogWarning($"{ClientOperationCode.VIEW_ALL_CHAR} is not implemented");
        }
    }
}