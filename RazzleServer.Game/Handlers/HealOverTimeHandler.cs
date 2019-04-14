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
                if ((DateTime.UtcNow - client.GameCharacter.LastHealthHealOverTime).TotalSeconds < 2)
                {
                    client.GameCharacter.LogCheatWarning(CheatType.InvalidHeal);
                    return;
                }

                client.GameCharacter.PrimaryStats.Health += healthAmount;
                client.GameCharacter.LastHealthHealOverTime = DateTime.UtcNow;
            }

            if (manaAmount != 0)
            {
                if ((DateTime.UtcNow - client.GameCharacter.LastManaHealOverTime).TotalSeconds < 2)
                {
                    client.GameCharacter.LogCheatWarning(CheatType.InvalidHeal);
                    return;
                }

                client.GameCharacter.PrimaryStats.Mana += manaAmount;
                client.GameCharacter.LastManaHealOverTime = DateTime.UtcNow;
            }
        }
    }
}
