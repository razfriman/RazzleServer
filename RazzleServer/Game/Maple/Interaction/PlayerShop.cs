using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class PlayerShop : MapObject, ISpawnable
    {
        public Character Owner { get; }
        public string Description { get; }
        public Character[] Visitors { get; }
        public List<PlayerShopItem> Items { get; }
        public bool Opened { get; private set; }
        public bool IsPrivate { get; } = false;

        public bool IsFull => Visitors.All(t => t != null);

        public PlayerShop(Character owner, string description)
        {
            Owner = owner;
            Description = description;
            Visitors = new Character[3];
            Items = new List<PlayerShopItem>();
            Opened = false;

            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                oPacket.WriteByte((byte)InteractionCode.Room);
                oPacket.WriteByte(4);
                oPacket.WriteByte(4);
                oPacket.WriteByte(0);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(Owner.AppearanceToByteArray());
                oPacket.WriteString(Owner.Name);
                oPacket.WriteByte(byte.MaxValue);
                oPacket.WriteString(Description);
                oPacket.WriteByte(16);
                oPacket.WriteByte(0);

                Owner.Client.Send(oPacket);
            }
        }

        public void Handle(Character character, InteractionCode code, PacketReader iPacket)
        {
            switch (code)
            {
                case InteractionCode.OpenStore:
                {
                    Owner.Map.PlayerShops.Add(this);

                    Opened = true;
                }
                    break;

                case InteractionCode.AddItem:
                {
                    var type = (ItemType)iPacket.ReadByte();
                    var slot = iPacket.ReadShort();
                    var bundles = iPacket.ReadShort();
                    var perBundle = iPacket.ReadShort();
                    var price = iPacket.ReadInt();
                    var quantity = (short)(bundles * perBundle);

                    var item = character.Items[type, slot];

                    if (item == null)
                    {
                        return;
                    }

                    if (perBundle < 0 || perBundle * bundles > 2000 || bundles < 0 || price < 0)
                    {
                        return;
                    }

                    if (quantity > item.Quantity)
                    {
                        return;
                    }

                    if (quantity < item.Quantity)
                    {
                        item.Quantity -= quantity;
                        item.Update();
                    }
                    else
                    {
                        character.Items.Remove(item, true);
                    }

                    var shopItem = new PlayerShopItem(item.MapleId, bundles, quantity, price);

                    Items.Add(shopItem);

                    UpdateItems();
                }
                    break;

                case InteractionCode.RemoveItem:
                {
                    if (character == Owner)
                    {
                        var slot = iPacket.ReadShort();

                        var shopItem = Items[slot];

                        if (shopItem == null)
                        {
                            return;
                        }

                        if (shopItem.Quantity > 0)
                        {
                            Owner.Items.Add(new Item(shopItem.MapleId, shopItem.Quantity));
                        }

                        Items.Remove(shopItem);

                        UpdateItems();
                    }
                }
                    break;

                case InteractionCode.Exit:
                {
                    if (character == Owner)
                    {
                        Close();
                    }
                    else
                    {
                        RemoveVisitor(character);
                    }
                }
                    break;

                case InteractionCode.Buy:
                {
                    short slot = iPacket.ReadByte();
                    var quantity = iPacket.ReadShort();

                    var shopItem = Items[slot];

                    if (shopItem == null)
                    {
                        return;
                    }

                    if (character == Owner)
                    {
                        return;
                    }

                    if (quantity > shopItem.Quantity)
                    {
                        return;
                    }

                    if (character.PrimaryStats.Meso < shopItem.MerchantPrice * quantity)
                    {
                        return;
                    }

                    shopItem.Quantity -= quantity;

                    character.PrimaryStats.Meso -= shopItem.MerchantPrice * quantity;
                    Owner.PrimaryStats.Meso += shopItem.MerchantPrice * quantity;

                    character.Items.Add(new Item(shopItem.MapleId, quantity));

                    UpdateItems(); // TODO: This doesn't mark the item as sold.

                    var noItemLeft = true;

                    foreach (var loopShopItem in Items)
                    {
                        if (loopShopItem.Quantity > 0)
                        {
                            noItemLeft = false;

                            break;
                        }
                    }

                    if (noItemLeft)
                    {
                        // TODO: Close the owner's shop.
                        // TODO: Notify  the owner the shop has been closed due to items being sold out.

                        Close();
                    }
                }
                    break;

                case InteractionCode.Chat:
                {
                    var text = iPacket.ReadString();

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Chat);
                        oPacket.WriteByte(8);

                        byte sender = 0;

                        for (var i = 0; i < Visitors.Length; i++)
                        {
                            if (Visitors[i] == character)
                            {
                                sender = (byte)(i + 1);
                            }
                        }


                        oPacket.WriteByte(sender);
                        oPacket.WriteString($"{character.Name} : {text}");

                        Broadcast(oPacket);
                    }
                }
                    break;
            }
        }

        public void Close()
        {
            foreach (var loopShopItem in Items)
            {
                if (loopShopItem.Quantity > 0)
                {
                    Owner.Items.Add(new Item(loopShopItem.MapleId, loopShopItem.Quantity));
                }
            }

            if (Opened)
            {
                Map.PlayerShops.Remove(this);

                foreach (var visitor in Visitors)
                {
                    if (visitor != null)
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.Exit);
                            oPacket.WriteByte(1);
                            oPacket.WriteByte(10);

                            visitor.Client.Send(oPacket);
                        }

                        visitor.PlayerShop = null;
                    }
                }
            }

            Owner.PlayerShop = null;
        }

        public void UpdateItems()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                oPacket.WriteByte((byte)InteractionCode.UpdateItems);
                oPacket.WriteByte((byte)Items.Count);

                foreach (var loopShopItem in Items)
                {
                    oPacket.WriteShort(loopShopItem.Bundles);
                    oPacket.WriteShort(loopShopItem.Quantity);
                    oPacket.WriteInt(loopShopItem.MerchantPrice);
                    oPacket.WriteBytes(loopShopItem.ToByteArray(true, true));
                }

                Broadcast(oPacket);
            }
        }

        public void Broadcast(PacketWriter oPacket, bool includeOwner = true)
        {
            if (includeOwner)
            {
                Owner.Client.Send(oPacket);
            }

            foreach (var visitor in Visitors)
            {
                visitor?.Client.Send(oPacket);
            }
        }

        public void AddVisitor(Character visitor)
        {
            for (var i = 0; i < Visitors.Length; i++)
            {
                if (Visitors[i] == null)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Visit);
                        oPacket.WriteByte((byte)(i + 1));
                        oPacket.WriteBytes(visitor.AppearanceToByteArray());
                        oPacket.WriteString(visitor.Name);

                        Broadcast(oPacket);
                    }

                    visitor.PlayerShop = this;
                    Visitors[i] = visitor;

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Room);
                        oPacket.WriteByte(4);
                        oPacket.WriteByte(4);
                        oPacket.WriteBool(true);
                        oPacket.WriteByte(0);
                        oPacket.WriteBytes(Owner.AppearanceToByteArray());
                        oPacket.WriteString(Owner.Name);

                        for (var slot = 0; slot < 3; slot++)
                        {
                            if (Visitors[slot] != null)
                            {
                                oPacket.WriteByte((byte)(slot + 1));
                                oPacket.WriteBytes(Visitors[slot].AppearanceToByteArray());
                                oPacket.WriteString(Visitors[slot].Name);
                            }
                        }


                        oPacket.WriteByte(byte.MaxValue);
                        oPacket.WriteString(Description);
                        oPacket.WriteByte(0x10);
                        oPacket.WriteByte((byte)Items.Count);

                        foreach (var loopShopItem in Items)
                        {
                            oPacket.WriteShort(loopShopItem.Bundles);
                            oPacket.WriteShort(loopShopItem.Quantity);
                            oPacket.WriteInt(loopShopItem.MerchantPrice);
                            oPacket.WriteBytes(loopShopItem.ToByteArray(true, true));
                        }

                        visitor.Client.Send(oPacket);
                    }

                    break;
                }
            }
        }

        public void RemoveVisitor(Character visitor)
        {
            for (var i = 0; i < Visitors.Length; i++)
            {
                if (Visitors[i] == visitor)
                {
                    visitor.PlayerShop = null;
                    Visitors[i] = null;

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Exit);

                        if (i > 0)
                        {
                            oPacket.WriteByte((byte)(i + 1));
                        }

                        Broadcast(oPacket, false);
                    }

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Exit);
                        oPacket.WriteByte((byte)(i + 1));

                        Owner.Client.Send(oPacket);
                    }

                    break;
                }
            }
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.AnnounceBox);


            oPacket.WriteInt(Owner.Id);
            oPacket.WriteByte(4);
            oPacket.WriteInt(ObjectId);
            oPacket.WriteString(Description);
            oPacket.WriteByte(0);
            oPacket.WriteByte(0);
            oPacket.WriteByte(1);
            oPacket.WriteByte(4);
            oPacket.WriteByte(0);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.AnnounceBox);

            oPacket.WriteInt(Owner.Id);
            oPacket.WriteByte(0);

            return oPacket;
        }
    }
}
