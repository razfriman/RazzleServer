using System;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.TeleportRockUse)]
    public class UseTeleportRockHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var action = (TeleportRockAction)packet.ReadByte();

            switch (action)
            {
                case TeleportRockAction.Remove:

                    var mapId = packet.ReadInt();
                    client.GameCharacter.TeleportRocks.Remove(mapId);
                    break;
                case TeleportRockAction.Add:

                    client.GameCharacter.TeleportRocks.Add(client.GameCharacter.Map.MapleId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
