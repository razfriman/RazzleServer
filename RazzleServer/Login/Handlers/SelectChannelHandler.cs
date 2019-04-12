using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SelectChannel)]
    public class SelectChannelHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            client.World = packet.ReadByte();
            client.Channel = packet.ReadByte();

            var channelResult = client.Server.Manager.Worlds[client.World]?.CheckChannel(client.Channel) ??
                                SelectChannelResult.Offline;
            var characters = client.Server.GetCharacters(client.World, client.Account.Id);

            using var pw = new PacketWriter(ServerOperationCode.CharacterList);
            pw.WriteByte(channelResult);

            if (channelResult == SelectChannelResult.Online)
            {
                pw.WriteByte((byte)characters.Count);
                characters.ForEach(x => pw.WriteBytes(x.ToByteArray()));
                pw.WriteLong(0);
            }

            client.Send(pw);
        }
    }
}
