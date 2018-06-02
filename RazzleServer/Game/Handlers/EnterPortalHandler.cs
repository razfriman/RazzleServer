﻿using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Scripts;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.EnterPortal)]
    public class EnterPortalHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                return;
            }

            var label = packet.ReadString();

            var portal = client.Character.Map.Portals[label];

            if (portal == null)
            {
                return;
            }

            ScriptProvider.Portals.Execute(portal, client.Character);
        }
    }
}