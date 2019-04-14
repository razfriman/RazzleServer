using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MobMovement)]
    public class MobMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();
            if (!client.GameCharacter.ControlledMobs.Contains(objectId))
            {
                // Monster is already dead
                return;
            }

            var mob = client.GameCharacter.ControlledMobs[objectId];
            mob?.Move(packet);
        }
    }
}
