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

        public static PacketWriter ShowStatusInfo(MessageType type, bool isMeso = false, int amount = 0, bool isWhite = false, bool inChat = false, int mapleId = 0, byte questAction = 0, string questString = null)
        {
            var pw = new PacketWriter(ServerOperationCode.Message);

            pw.WriteByte((byte)type);

            if (type == MessageType.DropPickup)
            {
                pw.WriteBool(isMeso);

                if (isMeso)
                {
                    pw.WriteInt(amount);
                    pw.WriteShort(0); // iNet Cafe Meso Gain
                }
                else
                {
                    pw.WriteInt(mapleId);
                    pw.WriteInt(amount);
                    pw.WriteInt(0);
                    pw.WriteInt(0);
                }
            }
            else if (type == MessageType.IncreaseMeso)
            {
                pw.WriteInt(amount);
                pw.WriteShort(0); // iNet Cafe Meso Gain
            }
            else if (type == MessageType.IncreaseExp)
            {
                pw.WriteBool(isWhite);
                pw.WriteInt(amount);
                pw.WriteBool(inChat);
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
            }
            else if (type == MessageType.IncreaseFame)
            {
                pw.WriteInt(amount);
            }
            else if (type == MessageType.QuestRecord)
            {
                pw.WriteShort(mapleId);
                pw.WriteByte(questAction);

                if (questAction == 0)
                {
                    // Cancel Quest
                    pw.WriteByte(0);
                    pw.WriteByte(0);
                    pw.WriteLong(0);
                }
                else if (questAction == 1)
                {
                    // Start Quest
                    pw.WriteString(questString);

                    if (questString != null)
                    {
                        pw.WriteLong(0);
                    }
                }
                else if (questAction == 2)
                {
                    // Complete Quest
                    pw.WriteByte(0);
                    pw.WriteDateTime(DateTime.Now);
                }
            }


            return pw;
        }
    }
}