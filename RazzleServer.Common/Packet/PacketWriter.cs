using System;
using System.IO;
using System.Text;
using RazzleServer.Common.Util;

namespace RazzleServer.Common.Packet
{
    /// <summary>
    /// Class to handle writing packets
    /// </summary>
    public class PacketWriter : APacket
    {
        public const int DefaultSize = 1024;

        /// <summary>
        /// The main writer tool
        /// </summary>
        private readonly BinaryWriter _binWriter;

        /// <summary>
        /// Amount of data writen in the writer
        /// </summary>
        public short Length => (short)Buffer.Length;

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        public PacketWriter()
        {
            Buffer = new MemoryStream(DefaultSize);
            _binWriter = new BinaryWriter(Buffer, Encoding.ASCII);
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
        public PacketWriter(byte[] data)
        {
            Buffer = new MemoryStream(data);
            _binWriter = new BinaryWriter(Buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Restart writing from the point specified. This will overwrite data in the packet.
        /// </summary>
        /// <param name="length">The point of the packet to start writing from.</param>
        public void Reset(int length) => Buffer.Seek(length, SeekOrigin.Begin);

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
        /// Writes a string prefixed with a [short] length before it, to the stream
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteString(string writeValue)
        {
            writeValue ??= string.Empty;

            WriteShort(writeValue.Length);
            WriteString(writeValue, writeValue.Length);
        }

        /// <summary>
        /// Writes a string to the stream. Pads it with 0 until the specified length
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        /// <param name="length"></param>
        public void WriteString(string writeValue, int length)
        {
            _binWriter.Write(writeValue.ToCharArray());

            if (writeValue.Length < length)
            {
                WriteZeroBytes(length - writeValue.Length);
            }
        }

        /// <summary>
        /// Writes a hex-string to the stream
        /// </summary>
        /// <param name="pHexString">The hex-string to write</param>
        public void WriteHexString(string pHexString) => WriteBytes(Functions.HexToBytes(pHexString));

        public void WriteZeroBytes(int length) => WriteBytes(new byte[length]);

        public void WritePoint(Point? writeValue)
        {
            WriteShort(writeValue?.X ?? 0);
            WriteShort(writeValue?.Y ?? 0);
        }

        public void WriteHeader(ServerOperationCode header) => WriteUShort((ushort)header);

        public void WriteBox(Rectangle? box)
        {
            WriteInt(box?.Lt.X ?? 0);
            WriteInt(box?.Lt.Y ?? 0);
            WriteInt(box?.Rb.X ?? 0);
            WriteInt(box?.Rb.Y ?? 0);
        }

        public void WriteDateTime(DateTime item) =>
            WriteLong((long)(item.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds);

        public void WriteKoreanDateTime(DateTime item) => WriteLong(
            (long)(item.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds *
            10000 + 116444592000000000L);
    }
}
