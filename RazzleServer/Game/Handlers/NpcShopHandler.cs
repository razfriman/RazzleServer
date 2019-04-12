using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Life;
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

        private static void Buy(PacketReader packet, GameClient client, Npc shop)
        {
            var slot = packet.ReadShort();
            var mapleId = packet.ReadInt();
            var quantity = packet.ReadShort();

            var item = shop.ShopItems[mapleId];

            if (client.Character.PrimaryStats.Meso < item.Price * quantity)
            {
                return;
            }

            Item purchase;
            int price;

            if (item.IsRecharageable)
            {
                purchase = new Item(item.MapleId, item.MaxPerStack);
                price = item.Price;

                if (quantity > purchase.MaxPerStack)
                {
                    SendShopResult(client, ShopResult.RechargeIncorrectRequest);
                }
            }
            else
            {
                purchase = new Item(item.MapleId, quantity);

                if (purchase.Type == ItemType.Equipment && quantity != 1)
                {
                    client.Character.LogCheatWarning(CheatType.InvalidShop);
                    return;
                }

                price = item.Price * quantity;
            }

            if (slot < 0 || slot >= shop.ShopItems.Count)
            {
                SendShopResult(client, ShopResult.BuyUnknown);
                return;
            }

            if (item.Stock == 0)
            {
                SendShopResult(client, ShopResult.BuyNoStock);
                return;
            }

            if (price > client.Character.PrimaryStats.Meso)
            {
                SendShopResult(client, ShopResult.BuyNoMoney);
                return;
            }

            if (client.Character.Items.SpaceTakenBy(purchase) > client.Character.Items.RemainingSlots(purchase.Type))
            {
                SendShopResult(client, ShopResult.BuyUnknown);
                return;
            }

            client.Character.PrimaryStats.Meso -= price;
            client.Character.Items.Add(purchase);
            item.Stock -= quantity;
            SendShopResult(client, ShopResult.BuySuccess);
        }

        private static void Sell(PacketReader packet, GameClient client, Npc shop)
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
                    item.SalePrice + (int)(shop.ShopItems[item.MapleId].UnitRechargeRate * item.Quantity);
            }
            else
            {
                client.Character.PrimaryStats.Meso += item.SalePrice * quantity;
            }

            using var pw = new PacketWriter(ServerOperationCode.NpcShopResult);
            pw.WriteByte(8);
            client.Character.Send(pw);
        }

        private static void Recharge(PacketReader packet, GameClient client, Npc shop)
        {
            var slot = packet.ReadShort();
            var item = client.Character.Items[ItemType.Usable, slot];
            var price = (int)(shop.ShopItems[item.MapleId].UnitRechargeRate * (item.MaxPerStack - item.Quantity));

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

            using var pw = new PacketWriter(ServerOperationCode.NpcShopResult);
            pw.WriteByte(8);
            client.Character.Send(pw);
        }

        private static void SendShopResult(GameClient client, ShopResult result)
        {
            using var pw = new PacketWriter(ServerOperationCode.NpcShopResult);
            pw.WriteByte(result);
            client.Send(pw);
        }
    }
}
