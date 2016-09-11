using System;
using System.Text;

namespace RazzleServer.Packet
{
    /// <summary>
    /// Class to handle writing data to an byte array
    /// </summary>
    public class ArrayWriter
    {
        /// <summary>
        /// Buffer holding the packet data
        /// </summary>
        private byte[] Buffer { get; set; }

        /// <summary>
        /// Length of the packet
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The position to start reading on
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Creates a new instance of a ArrayWriter
        /// </summary>
        public ArrayWriter()
        {
            this.Buffer = new byte[0x50];
        }

        /// <summary>
        /// Prevents the buffer being to small
        /// </summary>
        private void EnsureCapacity(int length)
        {
            if (Position + length < this.Buffer.Length) return; //Return as quikly as posible
            byte[] newBuffer = new byte[this.Buffer.Length + 0x50];
            System.Buffer.BlockCopy(this.Buffer, 0, newBuffer, 0, this.Buffer.Length);
            this.Buffer = newBuffer;
            EnsureCapacity(length);
        }

        /// <summary>
        /// Writes bytes to the buffer
        /// </summary>
        public void WriteBytes(byte[] bytes)
        {
            int length = bytes.Length;
            if (bytes == null || length == 0)
                throw new ArgumentNullException("bytes", "Trying to write zero or null bytes");

            EnsureCapacity(length);
            System.Buffer.BlockCopy(bytes, 0, this.Buffer, this.Position, length);

            Length += length;
            Position += length;
        }

        /// <summary>
        /// Writes a bool to the buffer
        /// </summary>
        public void WriteBool(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// Writes a signed byte to the buffer
        /// </summary>
        public void WriteSByte(sbyte value)
        {
            sbyte[] signed = { value };
            WriteBytes((byte[])(Array)signed);
        }

        /// <summary>
        /// Writes a unsigned byte to the buffer
        /// </summary>
        public void WriteByte(byte value)
        {
            WriteBytes(new byte[1] { value });
        }

        /// <summary>
        /// Writes a signed short to the buffer
        /// </summary>
        public void WriteShort(short value)
        {
            WriteBytes(new byte[2] {
                (byte)value,
                (byte)(value >> 8)
            });
        }

        /// <summary>
        /// Writes a unsigned short to the buffer
        /// </summary>
        public void WriteUShort(ushort value)
        {
            WriteBytes(new byte[2] {
                (byte)value,
                (byte)(value >> 8)
            });
        }

        /// <summary>
        /// Writes a signed int to the buffer
        /// </summary>
        public void WriteInt(int value)
        {
            WriteBytes(new byte[4] {
                (byte)value,
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24)
            });
        }

        /// <summary>
        /// Writes a unsigned int to the buffer
        /// </summary>
        public void WriteUInt(uint value)
        {
            WriteBytes(new byte[4] {
                (byte)value,
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24)
            });
        }

        /// <summary>
        /// Writes a signed long to the buffer
        /// </summary>
        public void WriteLong(long value)
        {
            WriteBytes(new byte[8] {
                (byte)value,
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24),
                (byte)(value >> 32),
                (byte)(value >> 40),
                (byte)(value >> 48),
                (byte)(value >> 56)
            });
        }

        /// <summary>
        /// Writes a unsigned long to the buffer
        /// </summary>
        public void WriteULong(ulong value)
        {
            WriteBytes(new byte[8] {
                (byte)value,
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24),
                (byte)(value >> 32),
                (byte)(value >> 40),
                (byte)(value >> 48),
                (byte)(value >> 56)
            });
        }

        /// <summary>
        /// Writes a number of empty bytes to the buffer
        /// </summary>
        /// <param name="count">Number of empty (zero) bytes to write</param>
        public void WriteZeroBytes(int count)
        {
            WriteBytes(new byte[count]);
        }

        /// <summary>
        /// Write a string as maplestring to the buffer
        /// </summary>
        /// <param name="mString">String to write</param>
        public void WriteMapleString(string mString)
        {
            if (String.IsNullOrWhiteSpace(mString) || mString.Length == 0)
            {
                WriteZeroBytes(2);
                return;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(mString);
            WriteUShort((ushort)bytes.Length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Creates an byte array of the current ArrayWriter
        /// </summary>
        /// <param name="direct">If true, returns a direct reference of the buffer</param>
        public byte[] ToArray(bool direct = false)
        {
            if (direct)
                return this.Buffer;
            else
            {
                byte[] toRet = new byte[this.Length];
                System.Buffer.BlockCopy(this.Buffer, 0, toRet, 0, this.Length);
                return toRet;
            }
        }
    }
}