using NLog;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CLIENT_START)]
    public class ClientStartHandler : APacketHandler
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var message = packet.ReadMapleString();
            Log.Info($"Client Start Message: {message}");
        }
    }
}