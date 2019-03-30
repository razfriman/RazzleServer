using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public partial class Character
    {
        public byte[] ToByteArray(bool viewAllCharacters = false)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBytes(StatisticsToByteArray());
                pw.WriteBytes(AppearanceToByteArray());
                pw.WriteBool(IsRanked);

                if (IsRanked)
                {
                    pw.WriteInt(Rank);
                    pw.WriteInt(RankMove);
                    pw.WriteInt(JobRank);
                    pw.WriteInt(JobRankMove);
                }

                return pw.ToArray();
            }
        }

        public byte[] StatisticsToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(Id);
                oPacket.WriteString(Name, 13);
                oPacket.WriteByte((byte)PrimaryStats.Gender);
                oPacket.WriteByte(PrimaryStats.Skin);
                oPacket.WriteInt(PrimaryStats.Face);
                oPacket.WriteInt(PrimaryStats.Hair);
                oPacket.WriteLong(0); // Pet SN
                oPacket.WriteByte(PrimaryStats.Level);
                oPacket.WriteShort((short)PrimaryStats.Job);
                oPacket.WriteShort(PrimaryStats.Strength);
                oPacket.WriteShort(PrimaryStats.Dexterity);
                oPacket.WriteShort(PrimaryStats.Intelligence);
                oPacket.WriteShort(PrimaryStats.Luck);
                oPacket.WriteShort(PrimaryStats.Health);
                oPacket.WriteShort(PrimaryStats.MaxHealth);
                oPacket.WriteShort(PrimaryStats.Mana);
                oPacket.WriteShort(PrimaryStats.MaxMana);
                oPacket.WriteShort(PrimaryStats.AbilityPoints);
                oPacket.WriteShort(PrimaryStats.SkillPoints);
                oPacket.WriteInt(PrimaryStats.Experience);
                oPacket.WriteShort(PrimaryStats.Fame);
                oPacket.WriteInt(Map.MapleId);
                oPacket.WriteByte(SpawnPoint);
                oPacket.WriteLong(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt(0);

                return oPacket.ToArray();
            }
        }

        public byte[] AppearanceToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                var visibleLayer = new Dictionary<byte, int>();
                var hiddenLayer = new Dictionary<byte, int>();

                foreach (var item in Items.GetEquipped())
                {
                    var slot = item.AbsoluteSlot;

                    if (slot < 100 && !visibleLayer.ContainsKey(slot))
                    {
                        visibleLayer[slot] = item.MapleId;
                    }
                    else if (slot > 100 && slot != 111)
                    {
                        slot -= 100;

                        if (visibleLayer.ContainsKey(slot))
                        {
                            hiddenLayer[slot] = visibleLayer[slot];
                        }

                        visibleLayer[slot] = item.MapleId;
                    }
                    else if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = item.MapleId;
                    }
                }

                foreach (var entry in visibleLayer)
                {
                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(0);

                foreach (var entry in hiddenLayer)
                {
                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(0);

                return oPacket.ToArray();
            }
        }

        public byte[] DataToByteArray()
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
            var pw = new PacketWriter(ServerOperationCode.RemotePlayerEnterField);

            pw.WriteInt(Id);
            pw.WriteString(Name);
            pw.WriteBytes(Buffs.ToByteArray());
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
                pw.WriteByte((byte)InteractionType.PlayerShop);
                pw.WriteInt(PlayerShop.ObjectId);
                pw.WriteString(PlayerShop.Description);
                pw.WriteBool(PlayerShop.IsPrivate);
                pw.WriteByte(0);
                pw.WriteByte(1);
                pw.WriteByte((byte)(PlayerShop.IsFull ? 1 : 2)); // NOTE: Visitor availability.
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
            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerLeaveField))
            {
                pw.WriteInt(Id);
                return pw;
            }
        }
    }
}
