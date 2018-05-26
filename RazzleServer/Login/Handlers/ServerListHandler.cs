using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldList)]
    [PacketHandler(ClientOperationCode.WorldRelist)]
    public class ServerListHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            client.Send(LoginPackets.SendServerList(client.Server.Manager.Worlds));
            client.Send(LoginPackets.SendServerListEnd());
        }
    }
}