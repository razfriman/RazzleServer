using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SelectCharacter)]
    public class SelectCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var characterId = packet.ReadInt();

            client.Server.Manager.Migrate(client.Host, client.Account.Id, characterId);

            var host = client.Socket.HostBytes;
            var port = client.Server.Manager.Worlds[client.World][client.Channel].Port;

            using (var pw = new PacketWriter(ServerOperationCode.ClientConnectToServerLogin))
            {
                pw.WriteByte(0);
                pw.WriteByte(0);
                pw.WriteBytes(host);
                pw.WriteUShort(port);
                pw.WriteInt(characterId);
                pw.WriteByte(0); // Premium
                client.Send(pw);
            }
        }
    }
}
