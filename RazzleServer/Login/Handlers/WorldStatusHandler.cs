using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldStatus)]
    public class WorldStatusHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var worldId = packet.ReadByte();

            if (client.Server.Manager.Worlds.Contains(worldId))
            {
                var world = client.Server.Manager.Worlds[worldId];
                client.Send(LoginPackets.SendWorldStatus(world));
            }
        }
    }
}