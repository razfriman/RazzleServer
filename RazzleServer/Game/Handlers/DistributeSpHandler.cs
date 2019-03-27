using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.DistributeSp)]
    public class DistributeSpHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var mapleId = packet.ReadInt();

            if (client.Character.SkillPoints == 0 || !DataProvider.Skills.Data.ContainsKey(mapleId))
            {
                client.Character.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }
            
            if (!client.Character.Skills.Contains(mapleId))
            {
                client.Character.Skills.Add(new Skill(mapleId));
            }

            var skill = client.Character.Skills[mapleId];

            if (skill.CurrentLevel + 1 > skill.MaxLevel)
            {
                client.Character.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            client.Character.SkillPoints--;
            client.Character.Release();
            skill.CurrentLevel++;
        }
    }
}
