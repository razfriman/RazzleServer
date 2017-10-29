using System;

namespace RazzleServer.Common.Packet
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class InteroperabilityPacketHandlerAttribute : Attribute
    {
        public InteroperabilityPacketHandlerAttribute(InteroperabilityOperationCode header)
        {
            Header = header;
        }

        public InteroperabilityOperationCode Header { get; private set; }
    }
}