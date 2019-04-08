﻿using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MobMovement)]
    public class MobMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();
            if (!client.Character.ControlledMobs.Contains(objectId))
            {
                // Monster is already dead
                return;
            }

            var mob = client.Character.ControlledMobs[objectId];
            mob?.Move(packet);
        }
    }
}
