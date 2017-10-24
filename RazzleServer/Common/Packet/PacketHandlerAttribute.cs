using System;

namespace RazzleServer.Common.Packet
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class PacketHandlerAttribute : Attribute
    {
        readonly ClientOperationCode header;

        public PacketHandlerAttribute(ClientOperationCode header)
        {
            this.header = header;
        }

        public ClientOperationCode Header => header;
    }
}