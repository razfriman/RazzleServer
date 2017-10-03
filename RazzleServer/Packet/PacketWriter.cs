using System;
using System.Drawing;
using System.IO;
using System.Text;
using RazzleServer.Util;

namespace RazzleServer.Packet
{
	/// <summary>
	/// Class to handle writing packets
	/// </summary>
    public class PacketWriter : MapleLib.PacketLib.PacketWriter
	{
		 public PacketWriter(short header) => WriteShort(header);       
       
         public PacketWriter(SMSGHeader header) => WriteHeader(header);      
       
         public void WriteHeader(CMSGHeader header) => WriteUShort((ushort)header);      
       
         public void WriteHeader(SMSGHeader header) => WriteUShort((ushort)header);      
       
         public void WritePoint(Point value)
         {     
             WriteShort((short) value.X);       
             WriteShort((short) value.Y);       
         }     
       
         public void WriteBox(BoundingBox box)
         {     
             WriteInt(box.LeftTop.X);      
             WriteInt(box.LeftTop.Y);      
             WriteInt(box.RightBottom.X);      
             WriteInt(box.RightBottom.Y);      
         }

        public void WriteZeroBytes(int length) => WriteBytes(new byte[length]);

        public override string ToString() => Functions.ByteArrayToStr(ToArray());
	}
}