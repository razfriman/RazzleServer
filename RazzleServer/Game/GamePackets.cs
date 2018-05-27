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

        public static PacketWriter Cooldown(int skillId, int cooldownSeconds)
        {
            using (var pw = new PacketWriter(ServerOperationCode.Cooldown))
            {
                pw.WriteInt(skillId);
                pw.WriteShort((short)cooldownSeconds);
                return pw;
            }
        }

        public static PacketWriter ChangeSkillRecordResult(int skillId, int level, int maxLevel, DateTime expiration)
        {

            using (var pw = new PacketWriter(ServerOperationCode.ChangeSkillRecordResult))
            {
                pw.WriteByte(1);
                pw.WriteShort(1);
                pw.WriteInt(skillId);
                pw.WriteInt(level);
                pw.WriteInt(maxLevel);
                pw.WriteDateTime(expiration);
                pw.WriteByte(4);
                return pw;
            }
        }
    }
}