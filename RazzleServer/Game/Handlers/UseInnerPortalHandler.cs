﻿using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseInnerPortal)]
    public class UseInnerPortalHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();
            var name = packet.ReadString();
            var toPoint = packet.ReadPoint();
            var fromPoint = packet.ReadPoint();

            var portal = client.Character.Map.Portals[name];

            if (portal == null)
            {
                return;
            }

            var destPortal = client.Character.Map.Portals[portal.DestinationLabel];

            if (portals != client.Character.Portals || 
                portal.Position != fromPoint || 
                destPortal.Position != toPoint)
            {
                // Invalid portal
            }
        }
    }
}


