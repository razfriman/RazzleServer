﻿using System;
using Newtonsoft.Json;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class NpcShopItem
    {
        public int MapleId { get; }
        public byte Period { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public float UnitRechargeRate { get; set; }
        
        [JsonIgnore] public short MaxPerStack => DataProvider.Items.Data[MapleId].MaxPerStack;

        [JsonIgnore] public bool IsRecharageable => DataProvider.Items.Data[MapleId].IsRechargeable;
        
        public NpcShopItem(NpcShopItemReference reference)
        {
            MapleId = reference.MapleId;
            Price = reference.Price;
            Stock = reference.Stock;
            Period = reference.Period;
            UnitRechargeRate = reference.UnitRechargeRate;
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteInt(MapleId);
            pw.WriteInt(Price);
            if (IsRecharageable)
            {
                pw.WriteLong(BitConverter.DoubleToInt64Bits(UnitRechargeRate));
            }

            pw.WriteShort(MaxPerStack);


            return pw.ToArray();
        }
    }
}
