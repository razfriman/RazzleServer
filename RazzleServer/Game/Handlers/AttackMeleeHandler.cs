using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.AttackMelee)]
    public class AttackMeleeHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            client.GameCharacter.Attack(packet, AttackType.Melee);
        }
    }
}
