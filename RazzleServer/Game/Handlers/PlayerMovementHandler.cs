using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerMovement)]
    public class PlayerMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            var movements = new Movements(packet);

            client.Character.Position = movements.Position;
            client.Character.Foothold = movements.Foothold;
            client.Character.Stance = movements.Stance;

            using (var oPacket = new PacketWriter(ServerOperationCode.Move))
            {

                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteBytes(movements.ToByteArray());
                client.Character.Map.Send(oPacket, client.Character);
            }
        }
    }
}