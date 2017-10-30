﻿using RazzleServer.Common.PacketLib;

namespace RazzleServer.Common.Packet
{
    public abstract class APacketHandler<T> where T : AClient
    {
        public abstract void HandlePacket(PacketReader packet, T client);
    }
}