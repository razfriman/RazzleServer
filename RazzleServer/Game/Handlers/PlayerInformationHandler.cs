using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerInformation)]
    public class PlayerInformationHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var characterId = packet.ReadInt();
            var target = client.Character.Map.Characters[characterId];

            if (target == null || target.IsMaster && !client.Character.IsMaster)
            {
                return;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInformation))
            {
                oPacket.WriteInt(target.Id);
                oPacket.WriteByte(target.Level);
                oPacket.WriteShort((int)target.Job);
                oPacket.WriteShort(target.Fame);
                oPacket.WriteString(target.Guild?.Name ?? "-");
                // NOTE: Pets. // petid(int), petname(string) level(byte) closeness(short) fullness(byte) petequipitemid(int)
                oPacket.WriteBool(false);
                oPacket.WriteByte(0); // NOTE: Wishlist. // int for each item
                client.Send(oPacket);
            }
        }
    }
}
