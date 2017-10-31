using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CHAR_SELECT)]
    public class SelectCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var characterID = packet.ReadInt();
            client.MacAddresses = packet.ReadString().Split(new char[] { ',', ' ' });

            if (!client.Server.CenterConnection.Migrate(client.Host, client.Account.ID, characterID))
            {
                client.Terminate();
                return;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.SelectCharacterResult))
            {
                oPacket.WriteByte(0);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(client.Socket.HostBytes);
                oPacket.WriteUShort(client.Server.Worlds[client.World][client.Channel].Port);
                oPacket.WriteInt(characterID);
                oPacket.WriteInt(0);
                oPacket.WriteByte(0);
                client.Send(oPacket);
            }
        }
    }
}