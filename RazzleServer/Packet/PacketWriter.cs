using System;
using RazzleServer.Util;

namespace RazzleServer.Packet
{
    /// <summary>
    /// Class to handle writing data to a packet.
    /// </summary>
    public class PacketWriter : ArrayWriter
    {
        public PacketWriter(short header)
        {
            WriteShort(header);
        }

        public PacketWriter(SMSGHeader header)
        {
            WriteHeader(header);
        }

        public PacketWriter() { }

        public void WriteHeader(CMSGHeader header)
        {
            WriteUShort((ushort)header);
        }

        public void WriteHeader(SMSGHeader header)
        {
            WriteUShort((ushort)header);
        }

        public void WritePoint(Point value)
        {
            WriteShort((short)value.X);
            WriteShort((short)value.Y);
        }

        public void WriteBox(BoundingBox box)
        {
            WriteInt(box.LeftTop.X);
            WriteInt(box.LeftTop.Y);
            WriteInt(box.RightBottom.X);
            WriteInt(box.RightBottom.Y);
        }

        public void WriteStaticString(string s)
        {
            WriteBytes(Functions.ASCIIToBytes(s));
        }

        public void WriteStaticString(string s, int length)
        {
            byte[] bytes = Functions.ASCIIToBytes(s);
            byte[] copy = new byte[length];
            Buffer.BlockCopy(bytes, 0, copy, 0, Math.Min(bytes.Length, length));
            WriteBytes(copy);
        }

        public void WriteHexString(string s)
        {
            WriteBytes(Functions.HexToBytes(s));
        }

        public override string ToString()
        {
            return (Functions.ByteArrayToStr(this.ToArray()));
        }
    }
}