using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.HitReactor)]
    public class HitReactorHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var objectId = packet.ReadInt();

            var reactors = client.Character.Map.Reactors;
            if (!reactors.Contains(objectId))
            {
                return;
            }

            var characterPosition = packet.ReadPoint();
            var actionDelay = packet.ReadShort();
            var reactor = reactors[objectId];
            reactor.Hit(client.Character, actionDelay);
        }
    }
}