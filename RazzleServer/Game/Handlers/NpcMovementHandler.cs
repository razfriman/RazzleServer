using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcMovement)]
    public class NpcMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int objectID = packet.ReadInt();
            var npc = client.Character.ControlledNpcs[objectID];
            npc?.Move(packet);

        }
    }
}