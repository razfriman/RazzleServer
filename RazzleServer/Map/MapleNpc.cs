using MapleLib.PacketLib;
using System;

namespace RazzleServer.Map
{
    public class MapleNpc
    {
        public static PacketWriter GetNpcTalk(int npcId, byte msgType, ChatType chatType, string text, int diffNpc = 0, bool prev = false, bool next = false)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.NPC_TALK);
            pw.WriteByte(msgType);
            pw.WriteByte(4); //always 4?
            pw.WriteByte((byte)chatType);
            pw.WriteInt(npcId);
            if (((byte)chatType & 0x4) != 0)
            {
                pw.WriteInt(diffNpc);
            }
            pw.WriteMapleString(text);
            if (msgType != 5)
            {
                pw.WriteBool(prev);
                pw.WriteBool(next);
            }
            return pw;
        }

        public static PacketWriter GetNpcTalkNum(int NpcId, string Text, int Def, int Min, int Max) //outdated
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.NPC_TALK);
            pw.WriteByte(4);
            pw.WriteInt(NpcId);
            pw.WriteShort(4);
            pw.WriteMapleString(Text);
            pw.WriteInt(Def);
            pw.WriteInt(Min);
            pw.WriteInt(Max);
            pw.WriteInt(0);

            return pw;
        }
        public static PacketWriter GetNpcTalkAskText(int NpcId, string Text, int min, int max, string textboxText) //outdated
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.NPC_TALK);
            pw.WriteByte(4);
            pw.WriteInt(NpcId);
            pw.WriteByte(0);
            pw.WriteByte(3);
            pw.WriteByte(0);
            pw.WriteMapleString(Text);
            pw.WriteMapleString(textboxText);
            pw.WriteShort((short)min);
            pw.WriteShort((short)max);

            return pw;
        }

        public static PacketWriter GetPlayerNpcChat(byte MsgType, ChatType Type, string Text, bool Prev = false, bool Next = false) //outdated
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.NPC_TALK);
            pw.WriteByte(4);
            pw.WriteInt(0);
            pw.WriteByte(MsgType);
            pw.WriteByte((byte)Type);
            pw.WriteMapleString(Text);
            if (MsgType != 5)
            {
                pw.WriteBool(Prev);
                pw.WriteBool(Next);
            }
            return pw;
        }

        [Flags]
        public enum ChatType
        {
            None = 0,
            NoEsc = 1,
            PlayerSpeaks = 2,
            DiffNpc = 4,
            Unk = 8
        }
    }
}