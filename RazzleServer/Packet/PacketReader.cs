using System.Drawing;
using System.Text;

namespace RazzleServer.Packet
{
    /// <summary>
    /// Class to handle reading data from a packet
    /// </summary>
    public class PacketReader : ArrayReader
    {
        /// <summary>
        /// Creates a new instance of PacketReader
        /// </summary>
        /// <param name="data">Starting byte array</param>
        public PacketReader(byte[] data, int length = -1)
            : base(data, length)
        { }

        /// <summary>
        /// Reads the first two bytes from the stream, no matter what. If the position = 0, it advances two, otherwise, it does not change.
        /// </summary>
        /// <returns></returns>
        public ushort ReadHeader()
        {
            int oldPos = Position;
            Position = 0;
            ushort ret = ReadUShort();
            if (oldPos != 0)
                Position = oldPos;
            return ret;
        }

        /// <summary>
        /// Reads a point from the stream
        /// </summary>
        /// <returns>A point</returns>
        public Point ReadPoint()
        {
            short x = ReadShort();
            short y = ReadShort();
            Point ret = new Point(x, y);
            return ret;
        }

        /// <summary>
        /// Reads an ASCII string from the stream
        /// </summary>
        /// <param name="length">Amount of bytes</param>
        /// <returns>An ASCII string</returns>
        public string ReadStaticString(int length, char nullchar = '.')
        {
            return base.ReadString(length, nullchar);
        }

        /// <summary>
        /// Reads an ASCII string from the stream
        /// This one cuts the string off at the first null character
        /// </summary>
        /// <param name="length">Amount of bytes</param>
        /// <returns>An ASCII string</returns>
        public string ReadStaticString2(int length)
        {
            byte[] bytes = ReadBytes(length);
            string ret = Encoding.ASCII.GetString(bytes);
            int index = ret.IndexOf('\0');
            if (index != -1)
                ret = ret.Remove(index);
            return ret;
        }


        public override string ToString()
        {
            return "";
            //return (Functions.ByteArrayToStr(base.Buffer));
        }

        public string ToString(bool fromCurrentPosition = false)
        {
            if (!fromCurrentPosition)
            {
                return "";
                //return (Functions.ByteArrayToStr(base.Buffer));
            }
            else
            {
                byte[] tempBuffer = null;
                int savePosition = Position;
                tempBuffer = ReadBytes(this.Available);
                Position = savePosition;

                return "";
                //return Functions.ByteArrayToStr(tempBuffer);
            }
        }
    }
}