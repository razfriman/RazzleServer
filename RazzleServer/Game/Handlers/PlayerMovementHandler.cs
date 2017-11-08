using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerMovement)]
    public class PlayerMovementHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            //35 00

            //02 portals
            //2D 00 15 00 -- unk 

            //----------

            //03

            //00 
            //2D 00 2B 00 
            //00 00 2C 01
            //00 00 
            //12 
            //96 00 

            //00 
            //2D 00 35 00 
            //00 00 00 00 
            //00 00 
            //12 
            //1E 00 

            //00 
            //2D 00 35 00 
            //00 00 00 00 
            //00 00
            //12
            //4A 01 

            //11 
            //F0 
            //FF FF FF FF 
            //FF FF FF 0F 
          

            //2D 00 15 00 
            //2D 00 35 00

            byte portals = packet.ReadByte();

            var movements = new Movements(packet);

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