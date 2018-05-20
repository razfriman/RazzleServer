using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerInformation)]
    public class PlayerInformationHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt();
            var characterId = packet.ReadInt();

            var target = client.Character.Map.Characters[characterId];

            if (target == null || (target.IsMaster && !client.Character.IsMaster))
            {
                return;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.CharacterInformation))
            {
                oPacket.WriteInt(target.Id);
                oPacket.WriteByte(target.Level);
                oPacket.WriteShort((int)target.Job);
                oPacket.WriteShort(target.Fame);
                oPacket.WriteBool(false); // NOTE: Marriage.
                oPacket.WriteString(target.Guild?.Name ?? "-");
                oPacket.WriteByte(0); // NOTE: Pets.
                oPacket.WriteByte(0); // NOTE: Mount.
                oPacket.WriteByte(0); // NOTE: Wishlist.
                client.Send(oPacket);
            }
        }
    }
}