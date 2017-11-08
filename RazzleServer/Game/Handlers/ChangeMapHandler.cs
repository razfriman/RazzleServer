using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeMap)]
    public class ChangeMapHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            byte portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                return;
            }

            int mapID = packet.ReadInt();
            string portalLabel = packet.ReadString();
            packet.ReadByte(); // NOTE: Unknown.
            bool wheel = packet.ReadBool();

            switch (mapID)
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
                            client.Character.ChangeMap(portal.DestinationMapID, portal.Link.ID);
                        }
                    }
                    break;

                default: // NOTE: Admin '/m' command.
                    {
                        if (!client.Character.IsMaster)
                        {
                            return;
                        }

                        client.Character.ChangeMap(mapID);
                    }
                    break;
            }
        }
    }
}