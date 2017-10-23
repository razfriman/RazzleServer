using MapleLib.PacketLib;
using Microsoft.Extensions.Logging;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.VIEW_ALL_CHAR)]
    public class ViewAllCharactersHandler : APacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            Log.LogWarning($"{CMSGHeader.VIEW_ALL_CHAR} is not implemented");
        }
    }
}