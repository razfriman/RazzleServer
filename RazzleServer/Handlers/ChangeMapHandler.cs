using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CHANGE_MAP)]
    public class ChangeMapHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            MapleCharacter chr = client.Account.Character;
            if (chr.HP <= 0 || chr.ActionState == ActionState.DEAD)
            {
                chr.Revive(true);
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
            string portalName = packet.ReadMapleString();

            if (client.Account.Character.Map != null)
            {
                if (targetMap == -1)
                {
                    client.Account.Character.Map.EnterPortal(client, portalName);
                } else
                {
                    client.Account.Character.ChangeMap(targetMap, portalName);
                }
            }
        }
    }
}