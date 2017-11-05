using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.StrangeData)]
    public class StrangeDataHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var a = packet.ReadBool();
            var b = packet.ReadInt();

        }
    }
}