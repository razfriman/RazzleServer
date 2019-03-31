using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class Trade
    {
        public Character Owner { get; private set; }
        public Character Visitor { get; private set; }
        public int OwnerMeso { get; private set; }
        public int VisitorMeso { get; private set; }
        public List<Item> OwnerItems { get; }
        public List<Item> VisitorItems { get; }
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

            using (var pw = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                pw.WriteByte(InteractionCode.Room);
                pw.WriteByte(3);
                pw.WriteByte(2);
                pw.WriteByte(0); // NOTE: Player index.
                pw.WriteByte(0);
                pw.WriteBytes(Owner.AppearanceToByteArray());
                pw.WriteString(Owner.Name);
                pw.WriteByte(byte.MaxValue);
                Owner.Client.Send(pw);
            }
        }

        public void Complete()
        {
            if (Owner.Items.CouldReceive(VisitorItems) && Visitor.Items.CouldReceive(OwnerItems))
            {
                Owner.PrimaryStats.Meso += VisitorMeso;
                Visitor.PrimaryStats.Meso += OwnerMeso;

                Owner.Items.AddRange(VisitorItems);
                Visitor.Items.AddRange(OwnerItems);
            }
        }

        public void Cancel()
        {
            Owner.PrimaryStats.Meso += OwnerMeso;
            Visitor.PrimaryStats.Meso += VisitorMeso;

            Owner.Items.AddRange(OwnerItems);
            Visitor.Items.AddRange(VisitorItems);
        }

        public void Handle(Character character, InteractionCode code, PacketReader packet)
        {
            switch (code)
            {
                case InteractionCode.Invite:
                    Invite(packet);
                    break;

                case InteractionCode.Decline:
                    Decline(character);
                    break;

                case InteractionCode.Visit:
                    Visit(character);
                    break;

                case InteractionCode.SetItems:
                    SetItems(packet, character);
                    break;

                case InteractionCode.SetMeso:
                {
                    var meso = packet.ReadInt();

                    if (meso < 0 || meso > character.PrimaryStats.Meso)
                    {
                        return;
                    }

                    // TODO: The meso written in this packet is the added meso amount.
                    // The amount that should be written is the total amount.

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte(InteractionCode.SetMeso);
                        oPacket.WriteByte(0);
                        oPacket.WriteInt(meso);

                        if (character == Owner)
                        {
                            if (OwnerLocked)
                            {
                                return;
                            }

                            OwnerMeso += meso;
                            Owner.PrimaryStats.Meso -= meso;

                            Owner.Client.Send(oPacket);
                        }
                        else
                        {
                            if (VisitorLocked)
                            {
                                return;
                            }

                            VisitorMeso += meso;
                            Visitor.PrimaryStats.Meso -= meso;

                            Visitor.Client.Send(oPacket);
                        }
                    }

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte(InteractionCode.SetMeso);
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
                            oPacket.WriteByte(InteractionCode.Exit);
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
                            oPacket.WriteByte(InteractionCode.Exit);
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
                        oPacket.WriteByte(InteractionCode.Confirm);

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
                            oPacket.WriteByte(InteractionCode.Exit);
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
                    var text = packet.ReadString();

                    using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                    {
                        oPacket.WriteByte(InteractionCode.Chat);
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

        private void Visit(Character character)
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
                    oPacket.WriteByte(InteractionCode.Visit);
                    oPacket.WriteByte(1);
                    oPacket.WriteBytes(Visitor.AppearanceToByteArray());
                    oPacket.WriteString(Visitor.Name);

                    Owner.Client.Send(oPacket);
                }

                using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
                {
                    oPacket.WriteByte(InteractionCode.Room);
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

        private void Decline(Character character)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.PlayerInteraction))
            {
                oPacket.WriteByte(InteractionCode.Decline);
                oPacket.WriteByte(3);
                oPacket.WriteString(character.Name);

                Owner.Client.Send(oPacket);
            }

            Owner.Trade = null;
            Visitor.Trade = null;
            Owner = null;
            Visitor = null;
        }

        private void Invite(PacketReader packet)
        {
            var characterId = packet.ReadInt();

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
                    oPacket.WriteByte(InteractionCode.Decline);
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
                    oPacket.WriteByte(InteractionCode.Invite);
                    oPacket.WriteByte(3);
                    oPacket.WriteString(Owner.Name);
                    oPacket.WriteBytes(new byte[] {0xB7, 0x50, 0x00, 0x00});

                    Visitor.Client.Send(oPacket);
                }
            }
        }

        private void SetItems(PacketReader packet, Character character)
        {
            var type = (ItemType)packet.ReadByte();
            var slot = packet.ReadShort();
            var quantity = packet.ReadShort();
            var targetSlot = packet.ReadByte();

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
                oPacket.WriteByte(InteractionCode.SetItems);
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
                oPacket.WriteByte(InteractionCode.SetItems);
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
    }
}
