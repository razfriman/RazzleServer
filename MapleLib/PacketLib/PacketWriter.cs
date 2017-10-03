using System;
using System.IO;
using System.Text;

namespace MapleLib.PacketLib
{
	/// <summary>
	/// Class to handle writing packets
	/// </summary>
	public class PacketWriter : AbstractPacket
	{
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
		/// <param name="size">Starting size of the buffer</param>
        public PacketWriter(int size = 0)
		{
			_buffer = new MemoryStream(size);
			_binWriter = new BinaryWriter(_buffer, Encoding.ASCII);
		}

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

		/// <summary>
		/// Sets a byte in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The byte to set</param>
        public void SetByte(long index, int writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteByte((byte)writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a byte array in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The bytes to set</param>
        public void SetBytes(long index, byte[] writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteBytes(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a bool in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The bool to set</param>
        public void SetBool(long index, bool writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteBool(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a short in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The short to set</param>
        public void SetShort(long index, int writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteShort((short)writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets an int in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The int to set</param>
        public void SetInt(long index, int writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteInt(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a long in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The long to set</param>
        public void SetLong(long index, long writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteLong(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a long in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The long to set</param>
        public void SetString(long index, string writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteString(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a string prefixed with a [short] length before it, in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The string to set</param>
        public void SetMapleString(long index, string writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteMapleString(writeValue);
			_buffer.Position = oldIndex;
		}

		/// <summary>
		/// Sets a hex-string in the stream
		/// </summary>
		/// <param name="index">The index of the stream to set data at</param>
		/// <param name="writeValue">The hex-string to set</param>
        public void SetHexString(long index, string writeValue)
		{
			long oldIndex = _buffer.Position;
			_buffer.Position = index;
			WriteHexString(writeValue);
			_buffer.Position = oldIndex;
		}

	}
}