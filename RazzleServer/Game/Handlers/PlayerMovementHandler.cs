using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerMovement)]
    public class PlayerMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            byte portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                return;
            }

            packet.ReadInt(); // NOE: Unknown.

            var movements = Movements.Decode(packet);

            client.Character.Position = movements.Position;
            client.Character.Foothold = movements.Foothold;
            client.Character.Stance = movements.Stance;

            using (var oPacket = new PacketWriter(ServerOperationCode.Move))
            {

                oPacket.WriteInt(client.Character.ID);
                oPacket.WriteBytes(movements.ToByteArray());
                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}