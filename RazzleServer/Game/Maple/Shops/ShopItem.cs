﻿using System;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class ShopItem
    {
        public Shop Parent { get; private set; }

        public int MapleId { get; private set; }
        public short Quantity { get; private set; }
        public int PurchasePrice { get; private set; }
        public int Sort { get; private set; }

        public short MaxPerStack => DataProvider.Items.Data[MapleId].MaxPerStack;

        public int SalePrice => DataProvider.Items.Data[MapleId].SalePrice;

        public double UnitPrice => Parent.UnitPrices[MapleId];

        public bool IsRecharageable => DataProvider.Items.Data[MapleId].IsRechargeable;

        //public ShopItem(Shop parent, Datum datum)
        //{
        //    this.Parent = parent;

        //    this.MapleId = (int)datum["itemid"];
        //    this.Quantity = (short)datum["quantity"];
        //    this.PurchasePrice = (int)datum["price"];
        //    this.Sort = (int)datum["sort"];
        //}

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
