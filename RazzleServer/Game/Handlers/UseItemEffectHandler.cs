using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseItemEffect)]
    public class UseItemEffectHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int mapleID = packet.ReadInt();
            client.Character.ItemEffect = mapleID;
        }
    }
}