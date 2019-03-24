using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeChannel)]
    public class ChangeChannelHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var channel = packet.ReadByte();
            client.ChangeChannel(channel);

            Delay.Execute(() => client.Terminate($"Changing Channels: {client.Server.ChannelId} -> {channel}"), 5000);
        }
    }
}
