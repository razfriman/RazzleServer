using System;
using Newtonsoft.Json;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class ShopItem
    {
        public Shop Parent { get; }
        public int MapleId { get; }
        public short Quantity { get; }
        public int PurchasePrice { get; }
        public int Sort { get; private set; }

        [JsonIgnore] public short MaxPerStack => DataProvider.Items.Data[MapleId].MaxPerStack;

        [JsonIgnore] public int SalePrice => DataProvider.Items.Data[MapleId].SalePrice;

        [JsonIgnore] public double UnitPrice => Parent.UnitPrices[MapleId];

        [JsonIgnore] public bool IsRecharageable => DataProvider.Items.Data[MapleId].IsRechargeable;

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
            using (var pw = new PacketWriter())
            {
                pw.WriteInt(MapleId);
                pw.WriteInt(PurchasePrice);
                if (IsRecharageable)
                {
                    pw.WriteShort(0);
                    pw.WriteInt(0);
                    pw.WriteShort((short)(BitConverter.DoubleToInt64Bits(UnitPrice) >> 48));
                }
                else
                {
                    pw.WriteShort(Quantity);
                }

                pw.WriteShort(MaxPerStack);


                return pw.ToArray();
            }
        }
    }
}
