using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcShop)]
    public class NpcShopHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var shop = client.Character.LastNpc.Shop;

            var action = (ShopAction)packet.ReadByte();

            switch (action)
            {
                case ShopAction.Buy:
                    {
                        short index = packet.ReadShort();
                        int mapleID = packet.ReadInt();
                        short quantity = packet.ReadShort();

                        var item = shop.Items[index];

                        if (client.Character.Meso < item.PurchasePrice * quantity)
                        {
                            return;
                        }

                        Item purchase;
                        int price;

                        if (item.IsRecharageable)
                        {
                            purchase = new Item(item.MapleID, item.MaxPerStack);
                            price = item.PurchasePrice;
                        }
                        else if (item.Quantity > 1)
                        {
                            purchase = new Item(item.MapleID, item.Quantity);
                            price = item.PurchasePrice;
                        }
                        else
                        {
                            purchase = new Item(item.MapleID, quantity);
                            price = item.PurchasePrice * quantity;
                        }

                        if (client.Character.Items.SpaceTakenBy(purchase) > client.Character.Items.RemainingSlots(purchase.Type))
                        {
                            client.Character.Notify("Your inventory is full.", NoticeType.Popup);
                        }
                        else
                        {
                            client.Character.Meso -= price;
                            client.Character.Items.Add(purchase);
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.ConfirmShopTransaction))
                        {
                            oPacket.WriteByte(0);

                            client.Character.Client.Send(oPacket);
                        }
                    }
                    break;

                case ShopAction.Sell:
                    {
                        short slot = packet.ReadShort();
                        int mapleID = packet.ReadInt();
                        short quantity = packet.ReadShort();

                        Item item = client.Character.Items[mapleID, slot];

                        if (item.IsRechargeable)
                        {
                            quantity = item.Quantity;
                        }

                        if (quantity > item.Quantity)
                        {
                            return;
                        }
                        else if (quantity == item.Quantity)
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
                            client.Character.Meso += item.SalePrice + (int)(shop.UnitPrices[item.MapleID] * item.Quantity);
                        }
                        else
                        {
                            client.Character.Meso += item.SalePrice * quantity;
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.ConfirmShopTransaction))
                        {
                            oPacket.WriteByte(8);

                            client.Character.Client.Send(oPacket);
                        }
                    }
                    break;

                case ShopAction.Recharge:
                    {
                        short slot = packet.ReadShort();

                        Item item = client.Character.Items[ItemType.Usable, slot];

                        int price = (int)(shop.UnitPrices[item.MapleID] * (item.MaxPerStack - item.Quantity));

                        if (client.Character.Meso < price)
                        {
                            client.Character.Notify("You do not have enough mesos.", NoticeType.Popup);
                        }
                        else
                        {
                            client.Character.Meso -= price;

                            item.Quantity = item.MaxPerStack;
                            item.Update();
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.ConfirmShopTransaction))
                        {
                            oPacket.WriteByte(8);

                            client.Character.Client.Send(oPacket);
                        }
                    }
                    break;

                case ShopAction.Leave:
                    {
                        client.Character.LastNpc = null;
                    }
                    break;
            }
        }
    }
}


