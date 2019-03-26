using System;

namespace RazzleServer.Net.Packet
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class IgnorePacketPrintAttribute : Attribute
    {
    }
}
