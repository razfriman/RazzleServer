using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.AttackMelee)]
    public class AttackMeleeHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            client.Character.Attack(packet, AttackType.Melee);
        }
    }
}
