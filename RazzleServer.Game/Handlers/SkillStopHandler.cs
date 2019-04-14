using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.SkillStop)]
    public class SkillStopHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var skillId = packet.ReadInt();
            if (!client.GameCharacter.PrimaryStats.HasBuff(skillId))
            {
                return;
            }

            client.GameCharacter.PrimaryStats.RemoveByReference(skillId);
        }
    }
}
