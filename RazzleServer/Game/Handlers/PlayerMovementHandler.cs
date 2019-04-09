using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerMovement)]
    public class PlayerMovementHandler : GamePacketHandler
    {
        //TODO(raz): Packet Processing Error [PlayerMovement] 14 01 8E FF 8B 00 04 00 8E FF 8B 00 00 00 00 00 08 00 04 F0 00 08 01 08 01 00 8E FF 8B 00 00 00 00 00 08 00 04 0E 01 11 00 00 00 00 00 00 00 00 00 - Unable to read beyond the end of the stream. -    at System.IO.BinaryReader.InternalRead(Int32 numBytes)
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            var movements = new Movements(packet);

            client.Character.Position = movements.Position;
            client.Character.Foothold = movements.Foothold;
            client.Character.Stance = movements.Stance;

            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerMove))
            {
                pw.WriteInt(client.Character.Id);
                pw.WriteBytes(movements.ToByteArray());
                client.Character.Map.Send(pw, client.Character);
            }
        }
    }
}
