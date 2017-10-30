using System;
using System.IO;

namespace RazzleServer.Common.Packet
{
    public abstract class APacket : IDisposable
	{
		protected MemoryStream _buffer;

        public void Dispose()
        {
            _buffer.Close();
        }

        public byte[] ToArray() => _buffer.ToArray();
	}
}