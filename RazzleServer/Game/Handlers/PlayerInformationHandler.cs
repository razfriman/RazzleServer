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

            using var pw = new PacketWriter(ServerOperationCode.PlayerInformation);
            pw.WriteInt(target.Id);
            pw.WriteByte(target.PrimaryStats.Level);
            pw.WriteShort((int)target.PrimaryStats.Job);
            pw.WriteShort(target.PrimaryStats.Fame);
            // NOTE: Pets. // petid(int), petname(string) level(byte) closeness(short) fullness(byte) petequipitemid(int)
            pw.WriteBool(false);
            pw.WriteByte(0); // NOTE: Wishlist. // int for each item
            client.Send(pw);
        }
    }
}
