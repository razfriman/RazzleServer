using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Maps;

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

        public PacketWriter SendHint(string text, int width = 0, int height = 0)
        {
            using (var pw = new PacketWriter(ServerOperationCode.Hint))
            {
                if (width < 1)
                {
                    width = text.Length * 10;
                    if (width < 40)
                    {
                        width = 40;
                    }
                }

                if (height < 5)
                {
                    height = 5;
                }

                pw.WriteString(text);
                pw.WriteShort(width);
                pw.WriteShort(height);
                pw.WriteByte(1);
                Client.Send(pw);
            }
        }

        public byte[] StatisticsToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(Id);
                oPacket.WriteString(Name, 13);
                oPacket.WriteByte((byte)Gender);
                oPacket.WriteByte(Skin);
                oPacket.WriteInt(Face);
                oPacket.WriteInt(Hair);
                oPacket.WriteLong(0);
                oPacket.WriteLong(0);
                oPacket.WriteLong(0);
                oPacket.WriteByte(Level);
                oPacket.WriteShort((short)Job);
                oPacket.WriteShort(Strength);
                oPacket.WriteShort(Dexterity);
                oPacket.WriteShort(Intelligence);
                oPacket.WriteShort(Luck);
                oPacket.WriteShort(Health);
                oPacket.WriteShort(MaxHealth);
                oPacket.WriteShort(Mana);
                oPacket.WriteShort(MaxMana);
                oPacket.WriteShort(AbilityPoints);
                oPacket.WriteShort(SkillPoints);
                oPacket.WriteInt(Experience);
                oPacket.WriteShort(Fame);
                oPacket.WriteInt(0);
                oPacket.WriteInt(Map.MapleId);
                oPacket.WriteByte(SpawnPoint);
                oPacket.WriteInt(0);

                return oPacket.ToArray();
            }
        }

        public byte[] AppearanceToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                var megaphone = true;

                oPacket.WriteByte((int)Gender);
                oPacket.WriteByte(Skin);
                oPacket.WriteInt(Face);
                oPacket.WriteBool(megaphone);
                oPacket.WriteInt(Hair);

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

                oPacket.WriteByte(byte.MaxValue);

                foreach (var entry in hiddenLayer)
                {
                    oPacket.WriteByte(entry.Key);
                    oPacket.WriteInt(entry.Value);
                }

                oPacket.WriteByte(byte.MaxValue);

                var cashWeapon = Items[EquipmentSlot.CashWeapon];

                oPacket.WriteInt(cashWeapon?.MapleId ?? 0);

                oPacket.WriteInt(0); // pet id
                oPacket.WriteInt(0); // pet id
                oPacket.WriteInt(0); // pet id

                return oPacket.ToArray();
            }
        }

        public byte[] DataToByteArray()
        {
            var pw = new PacketWriter();
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteByte(BuddyListSlots);
            pw.WriteInt(Meso);
            pw.WriteBytes(Items.ToByteArray());// OK
            pw.WriteBytes(Skills.ToByteArray());
            pw.WriteBytes(Quests.ToByteArray());
            pw.WriteLong(0);// Rings
            pw.WriteBytes(Trocks.RegularToByteArray());
            pw.WriteBytes(Trocks.VipToByteArray());
            pw.WriteInt(0); // OK
            return pw.ToArray();
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.UserEnterField);

            oPacket.WriteInt(Id);
            oPacket.WriteByte(Level);
            oPacket.WriteString(Name);
            oPacket.WriteString(Guild?.Name);
            oPacket.WriteShort(Guild?.LogoBg ?? 0);
            oPacket.WriteByte(Guild?.LogoBgColor ?? 0);
            oPacket.WriteShort(Guild?.Logo ?? 0);
            oPacket.WriteByte(Guild?.LogoColor ?? 0);
            oPacket.WriteBytes(Buffs.ToByteArray());
            oPacket.WriteShort((short)Job);
            oPacket.WriteBytes(AppearanceToByteArray());
            oPacket.WriteInt(Items.Available(5110000));
            oPacket.WriteInt(ItemEffect);
            oPacket.WriteInt(Item.GetType(Chair) == ItemType.Setup ? Chair : 0);
            oPacket.WritePoint(Position);
            oPacket.WriteByte(Stance);
            oPacket.WriteShort(Foothold);
            oPacket.WriteByte(0);
            oPacket.WriteByte(0);
            oPacket.WriteInt(1);
            oPacket.WriteLong(0);

            if (PlayerShop != null && PlayerShop.Owner == this)
            {

                oPacket.WriteByte((byte)InteractionType.PlayerShop);
                oPacket.WriteInt(PlayerShop.ObjectId);
                oPacket.WriteString(PlayerShop.Description);
                oPacket.WriteBool(PlayerShop.IsPrivate);
                oPacket.WriteByte(0);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)(PlayerShop.IsFull ? 1 : 2)); // NOTE: Visitor availability.
                oPacket.WriteByte(0);
            }
            else
            {
                oPacket.WriteByte(0);
            }

            var hasChalkboard = !string.IsNullOrEmpty(Chalkboard);

            oPacket.WriteBool(hasChalkboard);

            if (hasChalkboard)
            {
                oPacket.WriteString(Chalkboard);
            }

            oPacket.WriteByte(0); // NOTE: Couple ring.
            oPacket.WriteByte(0); // NOTE: Friendship ring.
            oPacket.WriteByte(0); // NOTE: Marriage ring.
            oPacket.WriteByte(0);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            using (var pw = new PacketWriter(ServerOperationCode.UserLeaveField))
            {
                pw.WriteInt(Id);
                return pw;
            }
        }
    }
}
