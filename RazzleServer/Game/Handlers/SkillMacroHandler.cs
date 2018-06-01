using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.SkillMacro)]
    public class SkillMacroHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var count = packet.ReadByte();
        }
    }
}