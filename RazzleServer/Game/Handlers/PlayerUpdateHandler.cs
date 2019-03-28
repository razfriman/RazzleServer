using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerUpdate)]
    public class PlayerUpdateHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client) => client.Character.Save();
    }
}
