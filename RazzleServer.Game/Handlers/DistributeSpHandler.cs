using RazzleServer.Common.Constants;
using RazzleServer.DataProvider;
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

            if (client.GameCharacter.PrimaryStats.SkillPoints == 0 || !CachedData.Skills.Data.ContainsKey(mapleId))
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            if (!client.GameCharacter.Skills.Contains(mapleId))
            {
                client.GameCharacter.Skills.Add(new Skill(mapleId));
            }

            var skill = client.GameCharacter.Skills[mapleId];

            if (skill.CurrentLevel + 1 > skill.MaxLevel)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            client.GameCharacter.PrimaryStats.SkillPoints--;
            client.GameCharacter.Release();
            skill.CurrentLevel++;
        }
    }
}
