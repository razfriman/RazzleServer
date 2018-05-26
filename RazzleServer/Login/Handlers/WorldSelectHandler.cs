using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldSelect)]
    public class WorldSelectHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            client.World = packet.ReadByte();
            client.Channel = packet.ReadByte();

            var channelExists = Center.ServerManager.Instance.Worlds[client.World].Contains(client.Channel);
            var characters = client.Server.GetCharacters(client.World, client.Account.Id);

            client.Send(LoginPackets.SelectWord(channelExists, characters, client.Account));
        }
    }
}