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

            using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                pw.WriteByte(InteractionCode.Room);
                pw.WriteByte(4);
                pw.WriteByte(4);
                pw.WriteByte(0);
                pw.WriteByte(0);
                pw.WriteBytes(Owner.AppearanceToByteArray());
                pw.WriteString(Owner.Name);
                pw.WriteByte(byte.MaxValue);
                pw.WriteString(Description);
                pw.WriteByte(16);
                pw.WriteByte(0);

                Owner.Client.Send(pw);
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

                    using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        pw.WriteByte(InteractionCode.Chat);
                        pw.WriteByte(8);

                        byte sender = 0;

                        for (var i = 0; i < Visitors.Length; i++)
                        {
                            if (Visitors[i] == character)
                            {
                                sender = (byte)(i + 1);
                            }
                        }


                        pw.WriteByte(sender);
                        pw.WriteString($"{character.Name} : {text}");

                        Broadcast(pw);
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
                        using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            pw.WriteByte(InteractionCode.Exit);
                            pw.WriteByte(1);
                            pw.WriteByte(10);

                            visitor.Client.Send(pw);
                        }

                        visitor.PlayerShop = null;
                    }
                }
            }

            Owner.PlayerShop = null;
        }

        public void UpdateItems()
        {
            using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                pw.WriteByte(InteractionCode.UpdateItems);
                pw.WriteByte((byte)Items.Count);

                foreach (var loopShopItem in Items)
                {
                    pw.WriteShort(loopShopItem.Bundles);
                    pw.WriteShort(loopShopItem.Quantity);
                    pw.WriteInt(loopShopItem.MerchantPrice);
                    pw.WriteBytes(loopShopItem.ToByteArray(true, true));
                }

                Broadcast(pw);
            }
        }

        public void Broadcast(PacketWriter pw, bool includeOwner = true)
        {
            if (includeOwner)
            {
                Owner.Client.Send(pw);
            }

            foreach (var visitor in Visitors)
            {
                visitor?.Client.Send(pw);
            }
        }

        public void AddVisitor(Character visitor)
        {
            for (var i = 0; i < Visitors.Length; i++)
            {
                if (Visitors[i] == null)
                {
                    using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        pw.WriteByte(InteractionCode.Visit);
                        pw.WriteByte(i + 1);
                        pw.WriteBytes(visitor.AppearanceToByteArray());
                        pw.WriteString(visitor.Name);

                        Broadcast(pw);
                    }

                    visitor.PlayerShop = this;
                    Visitors[i] = visitor;

                    using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        pw.WriteByte(InteractionCode.Room);
                        pw.WriteByte(4);
                        pw.WriteByte(4);
                        pw.WriteBool(true);
                        pw.WriteByte(0);
                        pw.WriteBytes(Owner.AppearanceToByteArray());
                        pw.WriteString(Owner.Name);

                        for (var slot = 0; slot < 3; slot++)
                        {
                            if (Visitors[slot] != null)
                            {
                                pw.WriteByte(slot + 1);
                                pw.WriteBytes(Visitors[slot].AppearanceToByteArray());
                                pw.WriteString(Visitors[slot].Name);
                            }
                        }


                        pw.WriteByte(byte.MaxValue);
                        pw.WriteString(Description);
                        pw.WriteByte(0x10);
                        pw.WriteByte((byte)Items.Count);

                        foreach (var loopShopItem in Items)
                        {
                            pw.WriteShort(loopShopItem.Bundles);
                            pw.WriteShort(loopShopItem.Quantity);
                            pw.WriteInt(loopShopItem.MerchantPrice);
                            pw.WriteBytes(loopShopItem.ToByteArray(true, true));
                        }

                        visitor.Client.Send(pw);
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

                    using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        pw.WriteByte(InteractionCode.Exit);

                        if (i > 0)
                        {
                            pw.WriteByte(i + 1);
                        }

                        Broadcast(pw, false);
                    }

                    using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        pw.WriteByte(InteractionCode.Exit);
                        pw.WriteByte(i + 1);

                        Owner.Client.Send(pw);
                    }

                    break;
                }
            }
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.AnnounceBox);


            pw.WriteInt(Owner.Id);
            pw.WriteByte(4);
            pw.WriteInt(ObjectId);
            pw.WriteString(Description);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteByte(1);
            pw.WriteByte(4);
            pw.WriteByte(0);

            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.AnnounceBox);

            pw.WriteInt(Owner.Id);
            pw.WriteByte(0);

            return pw;
        }
    }
}
