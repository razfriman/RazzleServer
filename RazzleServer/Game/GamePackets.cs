using System;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game
{
    public static class GamePackets
    {
        internal static PacketWriter Ping()
        {
            return new PacketWriter(ServerOperationCode.Ping);
        }
    }
}