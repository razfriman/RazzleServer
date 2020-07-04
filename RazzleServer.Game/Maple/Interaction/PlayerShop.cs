using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class PlayerShop : IMapObject, ISpawnable
    {
        public GameCharacter Owner { get; }
        public string Description { get; }
        public GameCharacter[] Visitors { get; }
        public List<PlayerShopItem> Items { get; }
        public bool Opened { get; private set; }
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
        public bool IsPrivate { get; } = false;

        public bool IsFull => Visitors.All(t => t != null);

        public PlayerShop(GameCharacter owner, string description)
        {
            Owner = owner;
            Description = description;
            Visitors = new GameCharacter[3];
            Items = new List<PlayerShopItem>();
            Opened = false;

            using var pw = new PacketWriter(ServerOperationCode.PlayerInteraction);
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

            Owner.Send(pw);
        }

        public void Handle(GameCharacter gameCharacter, InteractionCode code, PacketReader iPacket)
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

                    var item = gameCharacter.Items[type, slot];

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
                        gameCharacter.Items.Remove(item, true);
                    }

                    var shopItem = new PlayerShopItem(item.MapleId, bundles, quantity, price);

                    Items.Add(shopItem);

                    UpdateItems();
                }
                    break;

                case InteractionCode.RemoveItem:
                {
                    if (gameCharacter == Owner)
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
                    if (gameCharacter == Owner)
                    {
                        Close();
                    }
                    else
                    {
                        RemoveVisitor(gameCharacter);
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

                    if (gameCharacter == Owner)
                    {
                        return;
                    }

                    if (quantity > shopItem.Quantity)
                    {
                        return;
                    }

                    if (gameCharacter.PrimaryStats.Meso < shopItem.MerchantPrice * quantity)
                    {
                        return;
                    }

                    shopItem.Quantity -= quantity;

                    gameCharacter.PrimaryStats.Meso -= shopItem.MerchantPrice * quantity;
                    Owner.PrimaryStats.Meso += shopItem.MerchantPrice * quantity;

                    gameCharacter.Items.Add(new Item(shopItem.MapleId, quantity));

                    UpdateItems(); // TODO: This doesn't mark the item as sold.

                    var noItemLeft = Items.All(loopShopItem => loopShopItem.Quantity <= 0);

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

                    using var pw = new PacketWriter(ServerOperationCode.PlayerInteraction);
                    pw.WriteByte(InteractionCode.Chat);
                    pw.WriteByte(8);

                    byte sender = 0;

                    for (var i = 0; i < Visitors.Length; i++)
                    {
                        if (Visitors[i] == gameCharacter)
                        {
                            sender = (byte)(i + 1);
                        }
                    }


                    pw.WriteByte(sender);
                    pw.WriteString($"{gameCharacter.Name} : {text}");

                    Broadcast(pw);
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
                        using var pw = new PacketWriter(ServerOperationCode.PlayerInteraction);
                        pw.WriteByte(InteractionCode.Exit);
                        pw.WriteByte(1);
                        pw.WriteByte(10);
                        visitor.Send(pw);
                        visitor.PlayerShop = null;
                    }
                }
            }

            Owner.PlayerShop = null;
        }

        public void UpdateItems()
        {
            using var pw = new PacketWriter(ServerOperationCode.PlayerInteraction);
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

        public void Broadcast(PacketWriter pw, bool includeOwner = true)
        {
            if (includeOwner)
            {
                Owner.Send(pw);
            }

            foreach (var visitor in Visitors)
            {
                visitor?.Send(pw);
            }
        }

        public void AddVisitor(GameCharacter visitor)
        {
            for (var i = 0; i < Visitors.Length; i++)
            {
                if (Visitors[i] == null)
                {
                    using var pw = new PacketWriter(ServerOperationCode.PlayerInteraction);
                    pw.WriteByte(InteractionCode.Visit);
                    pw.WriteByte(i + 1);
                    pw.WriteBytes(visitor.AppearanceToByteArray());
                    pw.WriteString(visitor.Name);
                    Broadcast(pw);

                    visitor.PlayerShop = this;
                    Visitors[i] = visitor;

                    using var pwVisitor = new PacketWriter(ServerOperationCode.PlayerInteraction);
                    pwVisitor.WriteByte(InteractionCode.Room);
                    pwVisitor.WriteByte(4);
                    pwVisitor.WriteByte(4);
                    pwVisitor.WriteBool(true);
                    pwVisitor.WriteByte(0);
                    pwVisitor.WriteBytes(Owner.AppearanceToByteArray());
                    pwVisitor.WriteString(Owner.Name);

                    for (var slot = 0; slot < 3; slot++)
                    {
                        if (Visitors[slot] != null)
                        {
                            pwVisitor.WriteByte(slot + 1);
                            pwVisitor.WriteBytes(Visitors[slot].AppearanceToByteArray());
                            pwVisitor.WriteString(Visitors[slot].Name);
                        }
                    }


                    pwVisitor.WriteByte(byte.MaxValue);
                    pwVisitor.WriteString(Description);
                    pwVisitor.WriteByte(0x10);
                    pwVisitor.WriteByte((byte)Items.Count);

                    foreach (var loopShopItem in Items)
                    {
                        pwVisitor.WriteShort(loopShopItem.Bundles);
                        pwVisitor.WriteShort(loopShopItem.Quantity);
                        pwVisitor.WriteInt(loopShopItem.MerchantPrice);
                        pwVisitor.WriteBytes(loopShopItem.ToByteArray(true, true));
                    }

                    visitor.Send(pwVisitor);

                    break;
                }
            }
        }

        public void RemoveVisitor(GameCharacter visitor)
        {
            for (var i = 0; i < Visitors.Length; i++)
            {
                if (Visitors[i] == visitor)
                {
                    visitor.PlayerShop = null;
                    Visitors[i] = null;

                    using var pwRemote = new PacketWriter(ServerOperationCode.PlayerInteraction);
                    pwRemote.WriteByte(InteractionCode.Exit);
                    if (i > 0)
                    {
                        pwRemote.WriteByte(i + 1);
                    }

                    Broadcast(pwRemote, false);

                    using var pwLocal = new PacketWriter(ServerOperationCode.PlayerInteraction);
                    pwLocal.WriteByte(InteractionCode.Exit);
                    pwLocal.WriteByte(i + 1);
                    Owner.Send(pwLocal);

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
