﻿using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.VIEW_ALL_CHAR)]
    public class ViewAllCharactersHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
        }
    }
}