using System;

namespace RazzleServer.Common.Packet
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class IgnorePacketPrintAttribute : Attribute
    {
    }
}