﻿using System.Collections.Generic;
using System.Linq;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class Shop
    {
        public int Id { get; private set; }
        public int NpcId { get; set; }
        public byte RechargeTier { get; set; }
        public List<ShopItem> Items { get; private set; }
        public Dictionary<int, double> UnitPrices => DataProvider.RechargeTiers.Data[RechargeTier];

        public Shop(ShopEntity entity)
        {
            Id = entity.ShopId;
            RechargeTier = entity.RechargeTier;
            Items = new List<ShopItem>();

            foreach (var item in entity.ShopItems.OrderBy(x => x.Sort))
            {
                if (DataProvider.Items.Data.ContainsKey(item.ItemId))
                {
                    Items.Add(new ShopItem(this, item));
                }
            }

            if (RechargeTier > 0)
            {
                foreach (var rechargeable in UnitPrices)
                {
                    Items.Add(new ShopItem(this, rechargeable.Key));
                }
            }
        }

        public void Show(Character customer)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.NpcShopShow))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteShort((short)Items.Count);
                Items.ForEach(x => oPacket.WriteBytes(x.ToByteArray()));
                customer.Client.Send(oPacket);
            }
        }
    }
}
