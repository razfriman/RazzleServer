using System;
using RazzleServer.Common.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeChannel)]
    public class ChangeChannelHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var channel = packet.ReadByte();
            client.ChangeChannel(channel);

            TaskRunner.Run(() => client.Terminate($"Changing Channels: {client.Server.ChannelId} -> {channel}"), TimeSpan.FromSeconds(5));
        }
    }
}
