using System;
using MapleLib.PacketLib;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.PONG)]
    public class PongHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client) => client.LastPong = DateTime.Now;

        public static PacketWriter PingPacket()
        {
            var pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.PING);
            return pw;
        }
    }
}