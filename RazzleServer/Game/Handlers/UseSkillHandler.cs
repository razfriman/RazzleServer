using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSkill)]
    public class UseSkillHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {

        }
    }
}