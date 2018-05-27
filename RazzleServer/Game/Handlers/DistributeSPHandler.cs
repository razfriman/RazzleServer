
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DistributeSp)]
    public class DistributeSpHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            if (client.Character.SkillPoints == 0)
            {
                return;
            }

            packet.ReadInt(); // NOTE: Ticks.
            var mapleId = packet.ReadInt();

            if (!client.Character.Skills.Contains(mapleId))
            {
                client.Character.Skills.Add(new Skill(mapleId));
            }

            var skill = client.Character.Skills[mapleId];

            if (skill.CurrentLevel + 1 <= skill.MaxLevel)
            {
                if (!skill.IsFromBeginner)
                {
                    client.Character.SkillPoints--;
                }

                client.Character.Release();

                skill.CurrentLevel++;
            }
        }
    }
}