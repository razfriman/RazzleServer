using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcMovement)]
    public class NpcMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();
            var npc = client.GameCharacter.ControlledNpcs[objectId];
            npc?.Move(packet);
        }
    }
}
