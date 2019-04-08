﻿using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Shops;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcShop)]
    public class NpcShopHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var shop = client.Character.CurrentNpcShop;

            if (shop == null)
            {
                client.Character.Release();
                return;
            }

            var action = (ShopAction)packet.ReadByte();

            switch (action)
            {
                case ShopAction.Buy:
                    Buy(packet, client, shop);
                    break;

                case ShopAction.Sell:
                    Sell(packet, client, shop);
                    break;

                case ShopAction.Recharge:
                    Recharge(packet, client, shop);
                    break;

                case ShopAction.Leave:
                    client.Character.CurrentNpcShop = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Buy(PacketReader packet, GameClient client, Shop shop)
        {
            var index = packet.ReadShort();
            var mapleId = packet.ReadInt();
            var quantity = packet.ReadShort();

            var item = shop.Items[index];

            if (client.Character.PrimaryStats.Meso < item.PurchasePrice * quantity)
            {
                return;
            }

            Item purchase;
            int price;

            if (item.IsRecharageable)
            {
                purchase = new Item(item.MapleId, item.MaxPerStack);
                price = item.PurchasePrice;
            }
            else if (item.Quantity > 1)
            {
                purchase = new Item(item.MapleId, item.Quantity);
                price = item.PurchasePrice;
            }
            else
            {
                purchase = new Item(item.MapleId, quantity);
                price = item.PurchasePrice * quantity;
            }

            if (client.Character.Items.SpaceTakenBy(purchase) > client.Character.Items.RemainingSlots(purchase.Type))
            {
                client.Character.Notify("Your inventory is full.", NoticeType.Popup);
            }
            else
            {
                client.Character.PrimaryStats.Meso -= price;
                client.Character.Items.Add(purchase);
            }

            using (var pw = new PacketWriter(ServerOperationCode.NpcShopResult))
            {
                pw.WriteByte(0);
                client.Character.Client.Send(pw);
            }
        }

        private static void Sell(PacketReader packet, GameClient client, Shop shop)
        {
            var slot = packet.ReadShort();
            var mapleId = packet.ReadInt();
            var quantity = packet.ReadShort();

            var item = client.Character.Items[mapleId, slot];

            if (item.IsRechargeable)
            {
                quantity = item.Quantity;
            }

            if (quantity > item.Quantity)
            {
                client.Character.LogCheatWarning(CheatType.InvalidItem);
                return;
            }

            if (quantity == item.Quantity)
            {
                client.Character.Items.Remove(item, true);
            }
            else if (quantity < item.Quantity)
            {
                item.Quantity -= quantity;
                item.Update();
            }

            if (item.IsRechargeable)
            {
                client.Character.PrimaryStats.Meso +=
                    item.SalePrice + (int)(shop.UnitPrices[item.MapleId] * item.Quantity);
            }
            else
            {
                client.Character.PrimaryStats.Meso += item.SalePrice * quantity;
            }

            using (var pw = new PacketWriter(ServerOperationCode.NpcShopResult))
            {
                pw.WriteByte(8);
                client.Character.Client.Send(pw);
            }
        }

        private static void Recharge(PacketReader packet, GameClient client, Shop shop)
        {
            var slot = packet.ReadShort();
            var item = client.Character.Items[ItemType.Usable, slot];
            var price = (int)(shop.UnitPrices[item.MapleId] * (item.MaxPerStack - item.Quantity));

            if (client.Character.PrimaryStats.Meso < price)
            {
                client.Character.Notify("You do not have enough mesos.", NoticeType.Popup);
            }
            else
            {
                client.Character.PrimaryStats.Meso -= price;
                item.Quantity = item.MaxPerStack;
                item.Update();
            }

            using (var pw = new PacketWriter(ServerOperationCode.NpcShopResult))
            {
                pw.WriteByte(8);
                client.Character.Client.Send(pw);
            }
        }
    }
}
