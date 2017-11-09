using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChannelChange)]
    public class ChangeChannelHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var channel = packet.ReadByte();
            client.ChangeChannel(channel);
            client.Terminate();
        }
    }
}