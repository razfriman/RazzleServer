using System;

namespace RazzleServer.Common.Packet
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class IgnorePacketPrintAttribute : Attribute
    {
    }
}