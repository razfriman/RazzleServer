using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CLIENT_START)]
    public class ClientStartHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var message = packet.ReadMapleString();
            System.Console.WriteLine($"Client Start Message: {message}");
        }
    }
}