using System;
using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.PONG)]
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