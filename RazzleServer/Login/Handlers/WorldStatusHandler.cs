using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldStatus)]
    public class WorldStatusHandler : LoginPacketHandler
    {
        private readonly ILogger _log = Log.ForContext<WorldStatusHandler>();

        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var worldId = packet.ReadByte();

            if (!client.Server.Manager.Worlds.Contains(worldId))
            {
                _log.Warning($"Cannot find world ID={worldId}");
                return;
            }

            var world = client.Server.Manager.Worlds[worldId];

            using (var pw = new PacketWriter(ServerOperationCode.WorldStatus))
            {
                pw.WriteShort((short)world.Status);
                client.Send(pw);
            }
        }
    }
}
