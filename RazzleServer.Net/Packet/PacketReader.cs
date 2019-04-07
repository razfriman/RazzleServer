﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using RazzleServer.Common.Util;

namespace RazzleServer.Net.Packet
{
    /// <summary>
    /// Class to handle reading data from a packet
    /// </summary>
    public class PacketReader : APacket
    {
        /// <summary>
        /// The main reader tool
        /// </summary>
        private readonly BinaryReader _binReader;

        /// <summary>
        /// Amount of data left in the reader
        /// </summary>
        public short Length => (short)Buffer.Length;

        public long Available => Buffer.Length - Buffer.Position;

        /// <inheritdoc />
        /// <summary>
        /// Creates a new instance of PacketReader
        /// </summary>
        /// <param name="arrayOfBytes">Starting byte array</param>
        public PacketReader(Span<byte> arrayOfBytes)
            : this(arrayOfBytes.ToArray())
        {
        }

        /// <summary>
        /// Creates a new instance of PacketReader
        /// </summary>
        /// <param name="arrayOfBytes">Starting byte array</param>
        public PacketReader(byte[] arrayOfBytes)
        {
            Buffer = new MemoryStream(arrayOfBytes, false);
            _binReader = new BinaryReader(Buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Restart reading from the point specified.
        /// </summary>
        /// <param name="position">The point of the packet to start reading from.</param>
        public void Seek(int position)
        {
            if (position < 0 || position > Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
            
            Buffer.Seek(position, SeekOrigin.Begin);
        }

        public void Skip(int length)
        {
            if (length < 0 || length > Available)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Buffer.Position += length;
        }

        /// <summary>
        /// Reads an unsigned byte from the stream
        /// </summary>
        /// <returns> an unsigned byte from the stream</returns>
        public byte ReadByte() => _binReader.ReadByte();

        /// <summary>
        /// Reads a signed byte from the stream
        /// </summary>
        /// <returns> a signed byte from the stream</returns>
        public sbyte ReadSByte() => _binReader.ReadSByte();

        /// <summary>
        /// Reads a byte array from the stream
        /// </summary>
        /// <param name="count">Amount of bytes</param>
        /// <returns>A byte array</returns>
        public byte[] ReadBytes(int count) => _binReader.ReadBytes(count);

        /// <summary>
        /// Reads a bool from the stream
        /// </summary>
        /// <returns>A bool</returns>
        public bool ReadBool() => _binReader.ReadBoolean();

        /// <summary>
        /// Reads a signed short from the stream
        /// </summary>
        /// <returns>A signed short</returns>
        public short ReadShort() => _binReader.ReadInt16();

        /// <summary>
        /// Reads a signed int from the stream
        /// </summary>
        /// <returns>A signed int</returns>
        public int ReadInt() => _binReader.ReadInt32();

        /// <summary>
        /// Reads a signed long from the stream
        /// </summary>
        /// <returns>A signed long</returns>
        public long ReadLong() => _binReader.ReadInt64();

        /// <summary>
        /// Reads an unsigned short from the stream
        /// </summary>
        /// <returns>An unsigned short</returns>
        public ushort ReadUShort() => _binReader.ReadUInt16();

        /// <summary>
        /// Reads an unsigned int from the stream
        /// </summary>
        /// <returns>An unsigned int</returns>
        public uint ReadUInt() => _binReader.ReadUInt32();

        /// <summary>
        /// Reads an unsigned long from the stream
        /// </summary>
        /// <returns>An unsigned long</returns>
        public ulong ReadULong() => _binReader.ReadUInt64();

        /// <summary>
        /// Reads an ASCII string from the stream
        /// </summary>
        /// <param name="length">Amount of bytes</param>
        /// <returns>An ASCII string</returns>
        public string ReadString(int length)
        {
            if (length > Available)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return Encoding.ASCII.GetString(ReadBytes(length).Where(x => x != 0x00).ToArray());
        }

        /// <summary>
        /// Reads a maple string from the stream
        /// </summary>
        /// <returns>A maple string</returns>
        public string ReadString() => ReadString(ReadShort());

        /// <summary>      
        /// Reads the first two bytes from the stream, no matter what. If the position = 0, it advances two, otherwise, it does not change.       
        /// </summary>        
        /// <returns></returns>       
        public byte ReadHeader()
        {
            var oldPos = Buffer.Position;
            Buffer.Position = 0;
            var ret = ReadByte();

            if (oldPos != 0)
            {
                Buffer.Position = oldPos;
            }

            return ret;
        }

        /// <summary>     
        /// Reads a point from the stream     
        /// </summary>        
        /// <returns>A point</returns>        
        public Point ReadPoint() => new Point(ReadShort(), ReadShort());
    }
}
