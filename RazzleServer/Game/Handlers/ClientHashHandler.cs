using RazzleServer.Common.Packet;


namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ClientHash)]
    public class ClientHashHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt();
        }
    }
}
