using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SelectWorld)]
    public class SelectWorldHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            client.World = packet.ReadByte();

            var channelResult = client.Server.Manager.Worlds[client.World].CheckChannel(client.Channel);
            var characters = client.Server.GetCharacters(client.World, client.Account.Id);

            var pw = new PacketWriter(ServerOperationCode.SelectWorldResult);

            pw.WriteByte((byte)channelResult);

            if (channelResult == SelectChannelResult.Online)
            {
                pw.WriteByte((byte)characters.Count);
                characters.ForEach(x => pw.WriteBytes(x.ToByteArray()));
                pw.WriteInt(client.Account.MaxCharacters);
            }

            client.Send(pw);
        }
    }
}
