using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public partial class GameCharacter
    {
        public override byte[] DataToByteArray()
        {
            var pw = new PacketWriter();
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteByte(PrimaryStats.BuddyListSlots);
            pw.WriteInt(PrimaryStats.Meso);
            pw.WriteBytes(Items.ToByteArray());
            pw.WriteBytes(Skills.ToByteArray());
            pw.WriteBytes(Quests.ToByteArray());
            pw.WriteShort(0); // Mini games (5 ints)
            pw.WriteBytes(Rings.ToByteArray());
            pw.WriteBytes(TeleportRocks.ToByteArray());
            return pw.ToArray();
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEnterField);
            pw.WriteInt(Id);
            pw.WriteString(Name);
            pw.WriteBytes(Buffs.ToMapBuffValues());
            pw.WriteShort((short)PrimaryStats.Job);
            pw.WriteBytes(AppearanceToByteArray());
            pw.WriteInt(Items.Available(5110000));
            pw.WriteInt(ItemEffect);
            pw.WriteInt(Item.GetType(Chair) == ItemType.Setup ? Chair : 0);
            pw.WritePoint(Position);
            pw.WriteByte(Stance);
            pw.WriteShort(Foothold);

            var pet = Pets.GetEquippedPet();
            pw.WriteBool(pet != null);
            if (pet != null)
            {
                pw.WriteInt(pet.Item.Id);
                pw.WriteString(pet.Name);
                pw.WriteLong(pet.Item.CashId);
                pw.WritePoint(pet.Position);
                pw.WriteByte(pet.Stance);
                pw.WriteShort(pet.Foothold);
            }

            if (PlayerShop != null && PlayerShop.Owner == this)
            {
                pw.WriteByte(InteractionType.PlayerShop);
                pw.WriteInt(PlayerShop.ObjectId);
                pw.WriteString(PlayerShop.Description);
                pw.WriteBool(PlayerShop.IsPrivate);
                pw.WriteByte(0);
                pw.WriteByte(1);
                pw.WriteByte(PlayerShop.IsFull ? 1 : 2);
                pw.WriteByte(0);
            }
            else
            {
                pw.WriteByte(0);
            }

            pw.WriteByte(0); // NOTE: Couple ring.
            pw.WriteByte(0); // NOTE: Friendship ring.
            pw.WriteByte(0); // NOTE: Marriage ring.
            pw.WriteByte(0);

            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerLeaveField);
            pw.WriteInt(Id);
            return pw;
        }
    }
}
