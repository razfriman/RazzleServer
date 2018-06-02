using System;

namespace RazzleServer.Common.Packet
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class IgnorePacketPrintAttribute : Attribute
    {
    }
}