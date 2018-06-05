using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Scripting;

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
            // 23 00 [01] [00 95 BA 0A] [00 00 00 00 00 
            var mapId = packet.ReadInt();
            var portalLabel = packet.ReadString();

            switch (mapId)
            {
                case 0:
                    Revive(client);
                    break;
                case -1:
                    UsePortal(client, portalLabel);
                    break;
                default:
                    ChangeMapAdmin(client, mapId);
                    break;
            }
        }

        private static void Revive(GameClient client)
        {
            if (client.Character.IsAlive)
            {
                return;
            }

            client.Character.Revive();
        }

        private static void ChangeMapAdmin(GameClient client, int mapId)
        {
            if (!client.Account.IsMaster)
            {
                return;
            }

            client.Character.ChangeMap(mapId);
        }

        private static void UsePortal(GameClient client, string portalLabel)
        {
            if (client.Character.Map.Portals.ContainsPortal(portalLabel))
            {
                var portal = client.Character.Map.Portals[portalLabel];

                if (portal.DestinationMapId == 999999999)
                {
                    ScriptProvider.Portals.Execute(portal, client.Character);
                }
                else
                {
                    client.Character.ChangeMap(portal.DestinationMapId, portal.Link.Id);
                }
            }
        }
    }
}