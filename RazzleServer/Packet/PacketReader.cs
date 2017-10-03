using System.Drawing;

namespace RazzleServer.Packet
{
	/// <summary>
	/// Class to handle reading data from a packet
	/// </summary>
    public class PacketReader : MapleLib.PacketLib.PacketReader
	{
	/// <summary>      
        /// Reads the first two bytes from the stream, no matter what. If the position = 0, it advances two, otherwise, it does not change.       
        /// </summary>        
        /// <returns></returns>       
        public ushort ReadHeader()
        {     
            var oldPos = _buffer.Position;
            _buffer.Position = 0;     
            ushort ret = ReadUShort();        
            if (oldPos != 0)      
                _buffer.Position = oldPos;        
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
	
	}
}