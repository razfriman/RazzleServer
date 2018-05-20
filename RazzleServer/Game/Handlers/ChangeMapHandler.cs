using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeMap)]
    public class ChangeMapHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                return;
            }
            var mapId = packet.ReadInt();
            var portalLabel = packet.ReadString();

            switch (mapId)
            {
                case 0: // NOTE: Death.
                    {
                        if (client.Character.IsAlive)
                        {
                            return;
                        }

                        client.Character.Revive();
                    }
                    break;

                case -1: // NOTE: Portal.
                    {

                        if (client.Character.Map.Portals.ContainsPortal(portalLabel))
                        {
                            var portal = client.Character.Map.Portals[portalLabel];
                            client.Character.ChangeMap(portal.DestinationMapId, portal.Link.Id);
                        }
                    }
                    break;

                default: // NOTE: Admin '/m' command.
                    {
                        if (!client.Account.IsMaster)
                        {
                            return;
                        }

                        client.Character.ChangeMap(mapId);
                    }
                    break;
            }
        }
    }
}