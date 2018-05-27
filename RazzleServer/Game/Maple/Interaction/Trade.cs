using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class Trade
    {
        public Character Owner { get; private set; }
        public Character Visitor { get; private set; }
        public int OwnerMeso { get; private set; }
        public int VisitorMeso { get; private set; }
        public List<Item> OwnerItems { get; private set; }
        public List<Item> VisitorItems { get; private set; }
        public bool Started { get; private set; }
        public bool OwnerLocked { get; private set; }
        public bool VisitorLocked { get; private set; }

        public Trade(Character owner)
        {
            Owner = owner;
            Visitor = null;
            OwnerMeso = 0;
            VisitorMeso = 0;
            OwnerItems = new List<Item>();
            VisitorItems = new List<Item>();
            Started = false;
            OwnerLocked = false;
            VisitorLocked = false;

            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                oPacket.WriteByte((byte)InteractionCode.Room);
                oPacket.WriteByte(3);
                oPacket.WriteByte(2);
                oPacket.WriteByte(0); // NOTE: Player index.
                oPacket.WriteByte(0);
                oPacket.WriteBytes(Owner.AppearanceToByteArray());
                oPacket.WriteString(Owner.Name);
                oPacket.WriteByte(byte.MaxValue);

                Owner.Client.Send(oPacket);
            }
        }

        public void Handle(Character character, InteractionCode code, PacketReader iPacket)
        {
            switch (code)
            {
                case InteractionCode.Invite:
                    {
                        var characterId = iPacket.ReadInt();

                        if (!Owner.Map.Characters.Contains(characterId))
                        {
                            // TODO: What does happen in case the invitee left?

                            return;
                        }

                        var invitee = Owner.Map.Characters[characterId];

                        if (invitee.Trade != null)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Decline);
                                oPacket.WriteByte(2);
                                oPacket.WriteString(invitee.Name);

                                Owner.Client.Send(oPacket);
                            }
                        }
                        else
                        {
                            invitee.Trade = this;
                            Visitor = invitee;

                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Invite);
                                oPacket.WriteByte(3);
                                oPacket.WriteString(Owner.Name);
                                oPacket.WriteBytes(new byte[] { 0xB7, 0x50, 0x00, 0x00 });

                                Visitor.Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case InteractionCode.Decline:
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.Decline);
                            oPacket.WriteByte(3);
                            oPacket.WriteString(character.Name);

                            Owner.Client.Send(oPacket);
                        }

                        Owner.Trade = null;
                        Visitor.Trade = null;
                        Owner = null;
                        Visitor = null;
                    }
                    break;

                case InteractionCode.Visit:
                    {
                        if (Owner == null)
                        {
                            Visitor = null;
                            character.Trade = null;

                            character.Notify("Trade has been closed.", NoticeType.Popup);
                        }
                        else
                        {
                            Started = true;

                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Visit);
                                oPacket.WriteByte(1);
                                oPacket.WriteBytes(Visitor.AppearanceToByteArray());
                                oPacket.WriteString(Visitor.Name);

                                Owner.Client.Send(oPacket);
                            }

                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Room);
                                oPacket.WriteByte(3);
                                oPacket.WriteByte(2);
                                oPacket.WriteByte(1);
                                oPacket.WriteByte(0);
                                oPacket.WriteBytes(Owner.AppearanceToByteArray());
                                oPacket.WriteString(Owner.Name);
                                oPacket.WriteByte(1);
                                oPacket.WriteBytes(Visitor.AppearanceToByteArray());
                                oPacket.WriteString(Visitor.Name);
                                oPacket.WriteByte(byte.MaxValue);

                                Visitor.Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case InteractionCode.SetItems:
                    {
                        var type = (ItemType)iPacket.ReadByte();
                        var slot = iPacket.ReadShort();
                        var quantity = iPacket.ReadShort();
                        var targetSlot = iPacket.ReadByte();

                        var item = character.Items[type, slot];

                        if (item.IsBlocked)
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

                            item = new Item(item.MapleId, quantity);
                        }
                        else
                        {
                            character.Items.Remove(item, true);
                        }

                        item.Slot = 0;

                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.SetItems);
                            oPacket.WriteByte(0);
                            oPacket.WriteByte(targetSlot);
                            oPacket.WriteBytes(item.ToByteArray(true));

                            if (character == Owner)
                            {
                                OwnerItems.Add(item);

                                Owner.Client.Send(oPacket);
                            }
                            else
                            {
                                VisitorItems.Add(item);

                                Visitor.Client.Send(oPacket);
                            }
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.SetItems);
                            oPacket.WriteByte(1);
                            oPacket.WriteByte(targetSlot);
                            oPacket.WriteBytes(item.ToByteArray(true));

                            if (character == Owner)
                            {
                                Visitor.Client.Send(oPacket);
                            }
                            else
                            {
                                Owner.Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case InteractionCode.SetMeso:
                    {
                        var meso = iPacket.ReadInt();

                        if (meso < 0 || meso > character.Meso)
                        {
                            return;
                        }

                        // TODO: The meso written in this packet is the added meso amount.
                        // The amount that should be written is the total amount.

                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.SetMeso);
                            oPacket.WriteByte(0);
                            oPacket.WriteInt(meso);

                            if (character == Owner)
                            {
                                if (OwnerLocked)
                                {
                                    return;
                                }

                                OwnerMeso += meso;
                                Owner.Meso -= meso;

                                Owner.Client.Send(oPacket);
                            }
                            else
                            {
                                if (VisitorLocked)
                                {
                                    return;
                                }

                                VisitorMeso += meso;
                                Visitor.Meso -= meso;

                                Visitor.Client.Send(oPacket);
                            }
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.SetMeso);
                            oPacket.WriteByte(1);
                            oPacket.WriteInt(meso);

                            if (Owner == character)
                            {
                                Visitor.Client.Send(oPacket);
                            }
                            else
                            {
                                oPacket.WriteInt(OwnerMeso);

                                Owner.Client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case InteractionCode.Exit:
                    {
                        if (Started)
                        {
                            Cancel();

                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Exit);
                                oPacket.WriteByte(0);
                                oPacket.WriteByte(2);

                                Owner.Client.Send(oPacket);
                                Visitor.Client.Send(oPacket);
                            }

                            Owner.Trade = null;
                            Visitor.Trade = null;
                            Owner = null;
                            Visitor = null;
                        }
                        else
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Exit);
                                oPacket.WriteByte(0);
                                oPacket.WriteByte(2);

                                Owner.Client.Send(oPacket);
                            }

                            Owner.Trade = null;
                            Owner = null;
                        }
                    }
                    break;

                case InteractionCode.Confirm:
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                        {
                            oPacket.WriteByte((byte)InteractionCode.Confirm);

                            if (character == Owner)
                            {
                                OwnerLocked = true;

                                Visitor.Client.Send(oPacket);
                            }
                            else
                            {
                                VisitorLocked = true;

                                Owner.Client.Send(oPacket);
                            }
                        }

                        if (OwnerLocked && VisitorLocked)
                        {
                            Complete();

                            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                            {
                                oPacket.WriteByte((byte)InteractionCode.Exit);
                                oPacket.WriteByte(0);
                                oPacket.WriteByte(6);

                                Owner.Client.Send(oPacket);
                                Visitor.Client.Send(oPacket);
                            }

                            Owner.Trade = null;
                            Visitor.Trade = null;
                            Owner = null;
                            Visitor = null;
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
                            oPacket.WriteBool(Owner != character);
                            oPacket.WriteString($"{character.Name} : {text}");

                            Owner.Client.Send(oPacket);
                            Visitor.Client.Send(oPacket);
                        }
                    }
                    break;
            }
        }

        public void Complete()
        {
            if (Owner.Items.CouldReceive(VisitorItems) && Visitor.Items.CouldReceive(OwnerItems))
            {
                Owner.Meso += VisitorMeso;
                Visitor.Meso += OwnerMeso;

                Owner.Items.AddRange(VisitorItems);
                Visitor.Items.AddRange(OwnerItems);
            }
        }

        public void Cancel()
        {
            Owner.Meso += OwnerMeso;
            Visitor.Meso += VisitorMeso;

            Owner.Items.AddRange(OwnerItems);
            Visitor.Items.AddRange(VisitorItems);
        }
    }
}
