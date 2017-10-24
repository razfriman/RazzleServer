using System.Drawing;
using System.IO;
using System.Text;
using RazzleServer.Util;

namespace RazzleServer.Common.Packet
{
    /// <summary>
    /// Class to handle writing packets
    /// </summary>
    public class PacketWriter : APacket
    {
        public const int DEFAULT_SIZE = 1024;

        /// <summary>
        /// The main writer tool
        /// </summary>
        private readonly BinaryWriter _binWriter;

        /// <summary>
        /// Amount of data writen in the writer
        /// </summary>
        public short Length => (short)_buffer.Length;

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        public PacketWriter()
        {
            _buffer = new MemoryStream(DEFAULT_SIZE);
            _binWriter = new BinaryWriter(_buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        public PacketWriter(ushort header) : this() => WriteUShort(header);

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        public PacketWriter(ServerOperationCode header) : this() => WriteHeader(header);

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        public PacketWriter(InteroperabilityOperationCode header) : this() => WriteHeader(header);

        public PacketWriter(byte[] data)
        {
            _buffer = new MemoryStream(data);
            _binWriter = new BinaryWriter(_buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Restart writing from the point specified. This will overwrite data in the packet.
        /// </summary>
        /// <param name="length">The point of the packet to start writing from.</param>
        public void Reset(int length) => _buffer.Seek(length, SeekOrigin.Begin);

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="writeValue">The byte to write</param>
        public void WriteByte(int writeValue) => _binWriter.Write((byte)writeValue);

        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="writeValue">The byte array to write</param>
        public void WriteBytes(byte[] writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a boolean to the stream
        /// </summary>
        /// <param name="writeValue">The boolean to write</param>
        public void WriteBool(bool writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="writeValue">The short to write</param>
        public void WriteShort(int writeValue) => _binWriter.Write((short)writeValue);

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="writeValue">The int to write</param>
        public void WriteInt(int writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a long to the stream
        /// </summary>
        /// <param name="writeValue">The long to write</param>
        public void WriteLong(long writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a ushort to the stream
        /// </summary>
        /// <param name="writeValue">The ushort to write</param>
        public void WriteUShort(ushort writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes an uint to the stream
        /// </summary>
        /// <param name="writeValue">The uint to write</param>
        public void WriteUInt(uint writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a ulong to the stream
        /// </summary>
        /// <param name="writeValue">The ulong to write</param>
        public void WriteULong(ulong writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a string to the stream
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteString(string writeValue) => _binWriter.Write(writeValue.ToCharArray());

        /// <summary>
        /// Writes a string to the stream. Pads it with 0 until the specified length
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteString(string writeValue, int length)
        {
            WriteString(writeValue);

            if (writeValue.Length < length)
            {
                WriteZeroBytes(length - writeValue.Length);
            }
        }

        /// <summary>
        /// Writes a string prefixed with a [short] length before it, to the stream
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteMapleString(string writeValue)
        {
            WriteShort((short)writeValue.Length);
            WriteString(writeValue);
        }

        /// <summary>
        /// Writes a hex-string to the stream
        /// </summary>
        /// <param name="pHexString">The hex-string to write</param>
        public void WriteHexString(string pHexString) => WriteBytes(HexEncoding.GetBytes(pHexString));

        public void WriteZeroBytes(int length) => WriteBytes(new byte[length]);

        public void WritePoint(Point writeValue)
        {
            WriteShort((short)writeValue.X);
            WriteShort((short)writeValue.Y);
        }

        public void WriteHeader(InteroperabilityOperationCode header) => WriteUShort((ushort)header);

        public void WriteHeader(ServerOperationCode header) => WriteUShort((ushort)header);

        public void WriteBox(BoundingBox box)
        {
            WriteInt(box.LeftTop.X);
            WriteInt(box.LeftTop.Y);
            WriteInt(box.RightBottom.X);
            WriteInt(box.RightBottom.Y);
        }

        public string ToPacketString() => Functions.ByteArrayToStr(ToArray());
    }
}