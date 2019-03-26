using System;

namespace RazzleServer.Net.Packet
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class PacketHandlerAttribute : Attribute
    {
        public PacketHandlerAttribute(ClientOperationCode header)
        {
            Header = header;
        }

        public ClientOperationCode Header { get; }
    }
}
