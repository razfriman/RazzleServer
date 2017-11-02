using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.WorldStatus)]
    public class ServerStatusHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var worldIndex = packet.ReadShort();
            var pw = new PacketWriter(ServerOperationCode.WorldInformation);
            pw.WriteShort((short)WorldStatus.Normal);
            client.Send(pw);
        }
    }
}