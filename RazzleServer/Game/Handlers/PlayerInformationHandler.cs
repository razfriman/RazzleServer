using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerInformation)]
    public class PlayerInformationHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt();
            int characterID = packet.ReadInt();

            var target = client.Character.Map.Characters[characterID];

            if (target == null || (target.IsMaster && !client.Character.IsMaster))
            {
                return;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.CharacterInformation))
            {
                oPacket.WriteInt(target.ID);
                oPacket.WriteByte(target.Level);
                oPacket.WriteShort((int)target.Job);
                oPacket.WriteShort(target.Fame);
                oPacket.WriteBool(false); // NOTE: Marriage.
                oPacket.WriteString(target.Guild?.Name ?? "-");
                oPacket.WriteByte(0); // NOTE: Unknown.
                oPacket.WriteByte(0); // NOTE: Pets.
                oPacket.WriteByte(0); // NOTE: Mount.
                oPacket.WriteByte(0); // NOTE: Wishlist.
                oPacket.WriteInt(0); // NOTE: Medal ID.
                oPacket.WriteShort(0); // NOTE: Medal quests.

                client.Send(oPacket);
            }
        }
    }
}