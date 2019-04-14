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

            if (portals != client.GameCharacter.Portals)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidPortals);
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
            if (client.GameCharacter.IsAlive)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidRevive);
                return;
            }

            client.GameCharacter.Revive();
        }

        private static void ChangeMapAdmin(GameClient client, int mapId)
        {
            if (!client.Account.IsMaster)
            {
                client.GameCharacter.LogCheatWarning(CheatType.ImperonatingGm);
                return;
            }

            client.GameCharacter.ChangeMap(mapId);
        }

        private static void UsePortal(GameClient client, string portalLabel)
        {
            var portal = client.GameCharacter.Map.Portals[portalLabel];
            portal.Enter(client.GameCharacter);
        }
    }
}
