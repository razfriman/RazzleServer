using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SelectCharacter)]
    [PacketHandler(ClientOperationCode.SelectCharacterByVAC)]
    public class SelectCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var characterId = packet.ReadInt();
            client.MacAddresses = packet.ReadString().Split(',', ' ');

            client.Server.Manager.Migrate(client.Host, client.Account.Id, characterId);

            using (var oPacket = new PacketWriter(ServerOperationCode.SelectCharacterResult))
            {
                oPacket.WriteByte(0);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(client.Socket.HostBytes);
                oPacket.WriteUShort(client.Server.Manager.Worlds[client.World][client.Channel].Port);
                oPacket.WriteInt(characterId);
                oPacket.WriteInt(0);
                oPacket.WriteByte(0);
                client.Send(oPacket);
            }
        }
    }
}