
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Skills;

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

            if (skill.IsFromBeginner)
            {
                var totalUsed = client.Character.Skills.GetCurrentLevel(1000)
                                      + client.Character.Skills.GetCurrentLevel(1001)
                                      + client.Character.Skills.GetCurrentLevel(1002);

                if (totalUsed < 6 && skill.CurrentLevel + 1 <= skill.MaxLevel)
                {
                    client.Character.SkillPoints--;
                    client.Character.Release();
                    skill.CurrentLevel++;
                }
            }
            else
            {
                if (skill.CurrentLevel + 1 <= skill.MaxLevel)
                {
                    client.Character.SkillPoints--;
                    client.Character.Release();
                    skill.CurrentLevel++;
                }
            }
        }
    }
}