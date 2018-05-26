using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game
{
    public static class GamePackets
    {
        public static PacketWriter Ping()
        {
            return new PacketWriter(ServerOperationCode.Ping);
        }

        public static PacketWriter Notify(string message, NoticeType type = NoticeType.Popup)
        {
            using (var pw = new PacketWriter(ServerOperationCode.BroadcastMsg))
            {
                pw.WriteByte((byte)type);

                if (type == NoticeType.Ticker)
                {
                    pw.WriteBool(!string.IsNullOrEmpty(message));
                }

                pw.WriteString(message);
                return pw;
            }
        }
    }
}