using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CHANGE_MAP)]
    public class ChangeMapHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            Character chr = client.Character;
            if (chr.Health <= 0)
            {
                chr.Revive();
                return;
            }

            if (!chr.DisableActions())
                return;

            if (packet.Available <= 0)
            {
                chr.EnableActions();
                return;
            }
            packet.Skip(1); // 1 = from dying, 2 = regular portal
            int targetMap = packet.ReadInt();
            string portalName = packet.ReadString();

            if (client.Character.Map != null)
            {
                if (targetMap == -1)
                {
                    client.Character.EnterPortal(portalName);
                } else
                {
                    client.Character.ChangeMap(targetMap, portalName);
                }
            }
        }
    }
}