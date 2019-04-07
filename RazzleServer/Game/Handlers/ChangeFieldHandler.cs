using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeField)]
    public class ChangeFieldHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                client.Character.LogCheatWarning(CheatType.InvalidPortals);
                return;
            }

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
                client.Character.LogCheatWarning(CheatType.InvalidRevive);
                return;
            }

            client.Character.Revive();
        }

        private static void ChangeMapAdmin(GameClient client, int mapId)
        {
            if (!client.Account.IsMaster)
            {
                client.Character.LogCheatWarning(CheatType.ImperonatingGm);
                return;
            }

            client.Character.ChangeMap(mapId);
        }

        private static void UsePortal(GameClient client, string portalLabel)
        {
            var portal = client.Character.Map.Portals[portalLabel];
            portal.Enter(client.Character);
        }
    }
}
