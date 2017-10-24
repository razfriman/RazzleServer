using System.IO;

namespace RazzleServer.Common.Packet
{
    public abstract class APacket
	{
		protected MemoryStream _buffer;

		public byte[] ToArray() => _buffer.ToArray();
	}
}