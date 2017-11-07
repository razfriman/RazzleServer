
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DistributeSP)]
    public class DistributeSPHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            if (client.Character.SkillPoints == 0)
            {
                return;
            }

            packet.ReadInt(); // NOTE: Ticks.
            int mapleID = packet.ReadInt();

            if (!client.Character.Skills.Contains(mapleID))
            {
                client.Character.Skills.Add(new Skill(mapleID));
            }

            var skill = client.Character.Skills[mapleID];

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