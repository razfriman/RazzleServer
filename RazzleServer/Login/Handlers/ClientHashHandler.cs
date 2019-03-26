using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.ClientHash)]
    public class ClientHashHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
        }
    }
}
