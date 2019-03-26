using System;
using System.IO;
using RazzleServer.Common.Util;

namespace RazzleServer.Net.Packet
{
    public abstract class APacket : IDisposable
    {
        protected MemoryStream Buffer;

        public void Dispose() => Buffer?.Close();

        public byte[] ToArray() => Buffer.ToArray();

        public string ToPacketString() => ToArray().ByteArrayToString();

    }
}
