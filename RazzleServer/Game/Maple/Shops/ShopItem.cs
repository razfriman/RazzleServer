using System;
using Newtonsoft.Json;
using RazzleServer.Common.Packet;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class ShopItem
    {
        public Shop Parent { get; }

        public int MapleId { get; }
        public short Quantity { get; }
        public int PurchasePrice { get; }
        public int Sort { get; private set; }

        [JsonIgnore]
        public short MaxPerStack => DataProvider.Items.Data[MapleId].MaxPerStack;

        [JsonIgnore]
        public int SalePrice => DataProvider.Items.Data[MapleId].SalePrice;

        [JsonIgnore]
        public double UnitPrice => Parent.UnitPrices[MapleId];

        [JsonIgnore]
        public bool IsRecharageable => DataProvider.Items.Data[MapleId].IsRechargeable;

        public ShopItem(Shop parent, ShopItemEntity entity)
        {
            Parent = parent;
            MapleId = entity.ItemId;
            Quantity = entity.Quantity;
            PurchasePrice = entity.Price;
            Sort = entity.Sort;
        }

        public ShopItem(Shop parent, int mapleId)
        {
            Parent = parent;

            MapleId = mapleId;
            Quantity = 1;
            PurchasePrice = 0;
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(MapleId);
                oPacket.WriteInt(PurchasePrice);
                oPacket.WriteInt(0); // NOTE: Perfect Pitch.
                oPacket.WriteInt(0); // NOTE: Time limit.
                oPacket.WriteInt(0); // NOTE: Unknown.

                if (IsRecharageable)
                {

                    oPacket.WriteShort(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteShort((short)(BitConverter.DoubleToInt64Bits(UnitPrice) >> 48));
                    oPacket.WriteShort(MaxPerStack);
                }
                else
                {

                    oPacket.WriteShort(Quantity);
                    oPacket.WriteShort(MaxPerStack);
                }

                return oPacket.ToArray();
            }
        }
    }
}
