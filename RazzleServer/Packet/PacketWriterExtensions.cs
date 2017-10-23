using RazzleServer.Util;
using MapleLib.PacketLib;

namespace RazzleServer.Packet
{
    public static class PacketWriterExtensions
	{
       
         public static void WriteHeader(this PacketWriter pw, CMSGHeader header) => pw.WriteUShort((ushort)header);      
       
         public static void WriteHeader(this PacketWriter pw, SMSGHeader header) => pw.WriteUShort((ushort)header);      
       
         public static void WriteBox(this PacketWriter pw, BoundingBox box)
         {     
             pw.WriteInt(box.LeftTop.X);      
             pw.WriteInt(box.LeftTop.Y);      
             pw.WriteInt(box.RightBottom.X);      
             pw.WriteInt(box.RightBottom.Y);      
         }

        public static string ToPacketString(this PacketWriter pw) => Functions.ByteArrayToStr(pw.ToArray());
	}
}