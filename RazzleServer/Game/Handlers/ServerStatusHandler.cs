using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.SERVERSTATUS_REQUEST)]
    public class ServerStatusHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var worldIndex = packet.ReadShort();
            var pw = new PacketWriter(ServerOperationCode.SERVERSTATUS);
            pw.WriteShort((short)WorldStatus.Normal);
            client.Send(pw);
        }
    }
}