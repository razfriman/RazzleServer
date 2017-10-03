using MapleLib.PacketLib;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using RazzleServer.Packet;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.GUEST_LOGIN_REQUEST)]
    public class GuestLoginHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pw = new PacketWriter(SMSGHeader.SEND_LINK);
            pw.WriteShort(0x100);
            pw.WriteInt(Functions.Random(999999));
            pw.WriteLong(0);
            pw.WriteHexString("40 E0 FD 3B 37 4F 01");
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow));
            pw.WriteInt(0);
            pw.WriteMapleString("https://google.com");
        }
    }
}