using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.HitReactor)]
    public class HitReactorHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int objectId = packet.ReadInt();

            var reactors = client.Character.Map.Reactors;
            if (!reactors.Contains(objectId))
            {
                return;
            }

            Point characterPosition = packet.ReadPoint();
            short actionDelay = packet.ReadShort();

            var reactor = reactors[objectId];
            reactor.Hit(client.Character, actionDelay);
        }
    }
}