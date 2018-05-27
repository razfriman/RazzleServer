using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SelectCharacter)]
    [PacketHandler(ClientOperationCode.SelectCharacterByVac)]
    public class SelectCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var characterId = packet.ReadInt();
            client.MacAddresses = packet.ReadString().Split(',', ' ');

            client.Server.Manager.Migrate(client.Host, client.Account.Id, characterId);

            var host = client.Socket.HostBytes;
            var port = client.Server.Manager.Worlds[client.World][client.Channel].Port;

            client.Send(LoginPackets.SelectCharacterResult(characterId, host, port));
        }
    }
}