using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class PlayerShop : MapObject, ISpawnable
    {
        public Character Owner { get; private set; }
        public string Description { get; private set; }
        public Character[] Visitors { get; private set; }
        public List<PlayerShopItem> Items { get; private set; }
        public bool Opened { get; private set; }
        public bool IsPrivate { get; private set; } = false;

        public bool IsFull
        {
            get
            {
                for (int i = 0; i < this.Visitors.Length; i++)
                {
                    if (this.Visitors[i] == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        public PlayerShop(Character owner, string description)
        {
            this.Owner = owner;
            this.Description = description;
            this.Visitors = new Character[3];
            this.Items = new List<PlayerShopItem>();
            this.Opened = false;

            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {

                oPacket.WriteByte((byte)InteractionCode.Room);
                oPacket.WriteByte(4);
                oPacket.WriteByte(4);
                oPacket.WriteByte(0);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(this.Owner.AppearanceToByteArray());
                oPacket.WriteString(this.Owner.Name);
                oPacket.WriteByte(byte.MaxValue);
                oPacket.WriteString(this.Description);
                oPacket.WriteByte(16);
                oPacket.WriteByte(0);

                this.Owner.Client.Send(oPacket);
            }
        }

        public void Handle(Character character, InteractionCode code, PacketReader iPacket)
        {
            switch (code)
            {
                case InteractionCode.OpenStore:
                    {
                        this.Owner.Map.PlayerShops.Add(this);

                        this.Opened = true;
                    }
                    break;

                case InteractionCode.AddItem:
                    {
                        ItemType type = (ItemType)iPacket.ReadByte();
                        short slot = iPacket.ReadShort();
                        short bundles = iPacket.ReadShort();
                        short perBundle = iPacket.ReadShort();
                        int price = iPacket.ReadInt();
                        short quantity = (short)(bundles * perBundle);

                        Item item = character.Items[type, slot];

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

                        PlayerShopItem shopItem = new PlayerShopItem(item.MapleID, bundles, quantity, price);

                        this.Items.Add(shopItem);

                        this.UpdateItems();
                    }
                    break;

                case InteractionCode.RemoveItem:
                    {
                        if (character == this.Owner)
                        {
                            short slot = iPacket.ReadShort();

                            PlayerShopItem shopItem = this.Items[slot];

                            if (shopItem == null)
                            {
                                return;
                            }

                            if (shopItem.Quantity > 0)
                            {
                                this.Owner.Items.Add(new Item(shopItem.MapleID, shopItem.Quantity));
                            }

                            this.Items.Remove(shopItem);

                            this.UpdateItems();
                        }
                    }
                    break;

                case InteractionCode.Exit:
                    {
                        if (character == this.Owner)
                        {
                            this.Close();
                        }
                        else
                        {
                            this.RemoveVisitor(character);
                        }
                    }
                    break;

                case InteractionCode.Buy:
                    {
                        short slot = iPacket.ReadByte();
                        short quantity = iPacket.ReadShort();

                        PlayerShopItem shopItem = this.Items[slot];

                        if (shopItem == null)
                        {
                            return;
                        }

                        if (character == this.Owner)
                        {
                            return;
                        }

                        if (quantity > shopItem.Quantity)
                        {
                            return;
                        }

                        if (character.Meso < shopItem.MerchantPrice * quantity)
                        {
                            return;
                        }

                        shopItem.Quantity -= quantity;

                        character.Meso -= shopItem.MerchantPrice * quantity;
                        this.Owner.Meso += shopItem.MerchantPrice * quantity;

                        character.Items.Add(new Item(shopItem.MapleID, quantity));

                        this.UpdateItems(); // TODO: This doesn't mark the item as sold.

                        bool noItemLeft = true;

                        foreach (PlayerShopItem loopShopItem in this.Items)
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

                            this.Close();
                        }
                    }
                    break;

                case InteractionCode.Chat:
                    {
                        string text = iPacket.ReadString();

                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {

                            oPacket.WriteByte((byte)InteractionCode.Chat);
                            oPacket.WriteByte(8);

                            byte sender = 0;

                            for (int i = 0; i < this.Visitors.Length; i++)
                            {
                                if (this.Visitors[i] == character)
                                {
                                    sender = (byte)(i + 1);
                                }
                            }


                            oPacket.WriteByte(sender);
                            oPacket.WriteString($"{character.Name} : {text}");

                            this.Broadcast(oPacket);
                        }
                    }
                    break;
            }
        }

        public void Close()
        {
            foreach (PlayerShopItem loopShopItem in this.Items)
            {
                if (loopShopItem.Quantity > 0)
                {
                    this.Owner.Items.Add(new Item(loopShopItem.MapleID, loopShopItem.Quantity));
                }
            }

            if (this.Opened)
            {
                this.Map.PlayerShops.Remove(this);

                for (int i = 0; i < this.Visitors.Length; i++)
                {
                    if (this.Visitors[i] != null)
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {

                            oPacket.WriteByte((byte)InteractionCode.Exit);
                            oPacket.WriteByte(1);
                            oPacket.WriteByte(10);

                            this.Visitors[i].Client.Send(oPacket);
                        }

                        this.Visitors[i].PlayerShop = null;
                    }
                }
            }

            this.Owner.PlayerShop = null;
        }

        public void UpdateItems()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {

                oPacket.WriteByte((byte)InteractionCode.UpdateItems);
                oPacket.WriteByte((byte)this.Items.Count);

                foreach (PlayerShopItem loopShopItem in this.Items)
                {

                    oPacket.WriteShort(loopShopItem.Bundles);
                    oPacket.WriteShort(loopShopItem.Quantity);
                    oPacket.WriteInt(loopShopItem.MerchantPrice);
                    oPacket.WriteBytes(loopShopItem.ToByteArray(true, true));
                }

                this.Broadcast(oPacket);
            }
        }

        public void Broadcast(PacketWriter oPacket, bool includeOwner = true)
        {
            if (includeOwner)
            {
                this.Owner.Client.Send(oPacket);
            }

            for (int i = 0; i < this.Visitors.Length; i++)
            {
                if (this.Visitors[i] != null)
                {
                    this.Visitors[i].Client.Send(oPacket);
                }
            }
        }

        public void AddVisitor(Character visitor)
        {
            for (int i = 0; i < this.Visitors.Length; i++)
            {
                if (this.Visitors[i] == null)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {

                        oPacket.WriteByte((byte)InteractionCode.Visit);
                        oPacket.WriteByte((byte)(i + 1));
                        oPacket.WriteBytes(visitor.AppearanceToByteArray());
                        oPacket.WriteString(visitor.Name);

                        this.Broadcast(oPacket);
                    }

                    visitor.PlayerShop = this;
                    this.Visitors[i] = visitor;

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {

                        oPacket.WriteByte((byte)InteractionCode.Room);
                        oPacket.WriteByte(4);
                        oPacket.WriteByte(4);
                        oPacket.WriteBool(true);
                        oPacket.WriteByte(0);
                        oPacket.WriteBytes(this.Owner.AppearanceToByteArray());
                        oPacket.WriteString(this.Owner.Name);

                        for (int slot = 0; slot < 3; slot++)
                        {
                            if (this.Visitors[slot] != null)
                            {

                                oPacket.WriteByte((byte)(slot + 1));
                                oPacket.WriteBytes(this.Visitors[slot].AppearanceToByteArray());
                                oPacket.WriteString(this.Visitors[slot].Name);
                            }
                        }


                        oPacket.WriteByte(byte.MaxValue);
                        oPacket.WriteString(this.Description);
                        oPacket.WriteByte(0x10);
                        oPacket.WriteByte((byte)this.Items.Count);

                        foreach (PlayerShopItem loopShopItem in this.Items)
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
            for (int i = 0; i < this.Visitors.Length; i++)
            {
                if (this.Visitors[i] == visitor)
                {
                    visitor.PlayerShop = null;
                    this.Visitors[i] = null;

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Exit);

                        if (i > 0)
                        {
                            oPacket.WriteByte((byte)(i + 1));
                        }

                        this.Broadcast(oPacket, false);
                    }

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte((byte)InteractionCode.Exit);
                        oPacket.WriteByte((byte)(i + 1));

                        this.Owner.Client.Send(oPacket);
                    }

                    break;
                }
            }
        }

        public PacketWriter GetCreatePacket()
        {
            return this.GetSpawnPacket();
        }

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.AnnounceBox);


            oPacket.WriteInt(this.Owner.ID);
            oPacket.WriteByte(4);
            oPacket.WriteInt(this.ObjectID);
            oPacket.WriteString(this.Description);
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

            oPacket.WriteInt(this.Owner.ID);
            oPacket.WriteByte(0);

            return oPacket;
        }
    }
}
