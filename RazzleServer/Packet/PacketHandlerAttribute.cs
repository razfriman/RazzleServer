using System;

namespace RazzleServer.Packet
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class PacketHandlerAttribute : Attribute
    {
        readonly CMSGHeader header;

        public PacketHandlerAttribute(CMSGHeader header)
        {
            this.header = header;
        }

        public CMSGHeader Header => header;
    }
}