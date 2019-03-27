using System;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.HealOverTime)]
    public class HealOverTimeHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt(); // NOTE: Ticks.
            var healthAmount = packet.ReadShort();
            var manaAmount = packet.ReadShort();

            if (healthAmount != 0)
            {
                if ((DateTime.Now - client.Character.LastHealthHealOverTime).TotalSeconds < 2)
                {
                    client.Character.LogCheatWarning(CheatType.InvalidHeal);
                    return;
                }

                client.Character.Health += healthAmount;
                client.Character.LastHealthHealOverTime = DateTime.Now;
            }

            if (manaAmount != 0)
            {
                if ((DateTime.Now - client.Character.LastManaHealOverTime).TotalSeconds < 2)
                {
                    client.Character.LogCheatWarning(CheatType.InvalidHeal);
                    return;
                }

                client.Character.Mana += manaAmount;
                client.Character.LastManaHealOverTime = DateTime.Now;
            }
        }
    }
}
