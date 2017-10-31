using System;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class ShopItem
    {
        public Shop Parent { get; private set; }

        public int MapleID { get; private set; }
        public short Quantity { get; private set; }
        public int PurchasePrice { get; private set; }
        public int Sort { get; private set; }

        public short MaxPerStack => DataProvider.Items[this.MapleID].MaxPerStack;

        public int SalePrice => DataProvider.Items[this.MapleID].SalePrice;

        public double UnitPrice => Parent.UnitPrices[this.MapleID];

        public bool IsRecharageable => DataProvider.Items[this.MapleID].IsRechargeable;

        public ShopItem(Shop parent, Datum datum)
        {
            this.Parent = parent;

            this.MapleID = (int)datum["itemid"];
            this.Quantity = (short)datum["quantity"];
            this.PurchasePrice = (int)datum["price"];
            this.Sort = (int)datum["sort"];
        }

        public ShopItem(Shop parent, int mapleID)
        {
            this.Parent = parent;

            this.MapleID = mapleID;
            this.Quantity = 1;
            this.PurchasePrice = 0;
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteInt(this.MapleID);
                oPacket.WriteInt(this.PurchasePrice);
                oPacket.WriteInt(0); // NOTE: Perfect Pitch.
                oPacket.WriteInt(0); // NOTE: Time limit.
                oPacket.WriteInt(0); // NOTE: Unknown.

                if (this.IsRecharageable)
                {

                    oPacket.WriteShort(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteShort((short)(BitConverter.DoubleToInt64Bits(this.UnitPrice) >> 48));
                    oPacket.WriteShort(this.MaxPerStack);
                }
                else
                {

                    oPacket.WriteShort(this.Quantity);
                    oPacket.WriteShort(this.MaxPerStack);
                }

                return oPacket.ToArray();
            }
        }
    }
}
