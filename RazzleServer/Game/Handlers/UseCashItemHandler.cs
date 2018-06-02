using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Items;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseCashItem)]
    public class UseCashItemHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.Character.Items[ItemType.Cash, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            var used = false;

            switch (item.MapleId) // TODO: Enum for these.
            {
                case 5040000: // NOTE: Teleport Rock.
                case 5040001: // NOTE: Coke Teleport Rock.
                case 5041000: // NOTE: VIP Teleport Rock.
                    {
                        used = client.Character.Trocks.Use(itemId, packet);
                    }
                    break;

                case 5050001: // NOTE: 1st Job SP Reset.
                case 5050002: // NOTE: 2nd Job SP Reset.
                case 5050003: // NOTE: 3rd Job SP Reset.
                case 5050004: // NOTE: 4th Job SP Reset.
                    {

                    }
                    break;

                case 5050000: // NOTE: AP Reset.
                    {
                        var statDestination = (StatisticType)packet.ReadInt();
                        var statSource = (StatisticType)packet.ReadInt();

                        client.Character.AddAbility(statDestination, 1, true);
                        client.Character.AddAbility(statSource, -1, true);

                        used = true;
                    }
                    break;

                case 5071000: // NOTE: Megaphone.
                    {
                        if (client.Character.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text = packet.ReadString();

                        var message = string.Format($"{client.Character.Name} : {text}"); // TODO: Include medal name.

                        // NOTE: In GMS, this sends to everyone on the current channel, not the map (despite the item's description).
                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.Megaphone);
                            oPacket.WriteString(message);
                            client.Character.Map.Send(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5072000: // NOTE: Super Megaphone.
                    {
                        if (client.Character.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text = packet.ReadString();
                        var whisper = packet.ReadBool();

                        var message = string.Format($"{client.Character.Name} : {text}"); // TODO: Include medal name.

                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.SuperMegaphone);
                            oPacket.WriteString(message);
                            oPacket.WriteByte(client.Character.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);
                            client.Character.Client.Server.World.Send(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5390000: // NOTE: Diablo Messenger.
                case 5390001: // NOTE: Cloud 9 Messenger.
                case 5390002: // NOTE: Loveholic Messenger.
                    {
                        if (client.Character.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text1 = packet.ReadString();
                        var text2 = packet.ReadString();
                        var text3 = packet.ReadString();
                        var text4 = packet.ReadString();
                        var whisper = packet.ReadBool();

                        using (var oPacket = new PacketWriter(ServerOperationCode.SetAvatarMegaphone))
                        {
                            oPacket.WriteInt(itemId);
                            oPacket.WriteString(client.Character.Name);
                            oPacket.WriteString(text1);
                            oPacket.WriteString(text2);
                            oPacket.WriteString(text3);
                            oPacket.WriteString(text4);
                            oPacket.WriteInt(client.Character.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);
                            oPacket.WriteBytes(client.Character.AppearanceToByteArray());
                            client.Character.Client.Server.World.Send(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5076000: // NOTE: Item Megaphone.
                    {
                        var text = packet.ReadString();
                        var whisper = packet.ReadBool();
                        var includeItem = packet.ReadBool();

                        Item targetItem = null;

                        if (includeItem)
                        {
                            var type = (ItemType)packet.ReadInt();
                            var targetSlot = packet.ReadShort();

                            targetItem = client.Character.Items[type, targetSlot];

                            if (targetItem == null)
                            {
                                return;
                            }
                        }

                        var message = string.Format($"{client.Character.Name} : {text}"); // TODO: Include medal name.

                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.ItemMegaphone);
                            oPacket.WriteString(message);
                            oPacket.WriteByte(client.Character.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);
                            oPacket.WriteByte((byte)(targetItem?.Slot ?? 0));

                            if (targetItem != null)
                            {
                                oPacket.WriteBytes(targetItem.ToByteArray(true));
                            }

                            client.Character.Client.Server.World.Send(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5077000: // NOTE: Art Megaphone.
                    {

                    }
                    break;

                case 5170000: // NOTE: Pet Name Tag.
                    {
                        //// TODO: Get the summoned pet.

                        //string name = iPacket.ReadString();

                        //using (var oPacket = new PacketWriter(ServerOperationCode.PetNameChanged))
                        //{
                        //    oPacket
                        //        oPacket.WriteInt(this.client.Character.Id)
                        //        oPacket.WriteByte() // NOTE: Index.
                        //        oPacket.WriteString(name)
                        //        oPacket.WriteByte();

                        //    this.client.Character.Map.Send(oPacket);
                        //}
                    }
                    break;

                case 5060000: // NOTE: Item Name Tag.
                    {
                        var targetSlot = packet.ReadShort();

                        if (targetSlot == 0)
                        {
                            return;
                        }

                        var targetItem = client.Character.Items[ItemType.Equipment, targetSlot];

                        if (targetItem == null)
                        {
                            return;
                        }

                        targetItem.Creator = client.Character.Name;
                        targetItem.Update(); // TODO: This does not seem to update the item's creator.

                        used = true;
                    }
                    break;

                case 5520000: // NOTE: Scissors of Karma.
                case 5060001: // NOTE: Item Lock. 
                    {

                    }
                    break;

                case 5075000: // NOTE: Maple TV Messenger.
                case 5075003: // NOTE: Megassenger.
                    {

                    }
                    break;

                case 5075001: // NOTE: Maple TV Star Messenger.
                case 5075004: // NOTE: Star Megassenger.
                    {

                    }
                    break;

                case 5075002: // NOTE: Maple TV Heart Messenger.
                case 5075005: // NOTE: Heart Megassenger.
                    {

                    }
                    break;

                case 5200000: // NOTE: Bronze Sack of Meso.
                case 5200001: // NOTE: Silver Sack of Meso.
                case 5200002: // NOTE: Gold Sack of Meso.
                    {
                        client.Character.Meso += item.Meso;
                        client.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, true, item.Meso));
                        used = true;
                    }
                    break;

                case 5370000: // NOTE: Chalkboard.
                case 5370001: // NOTE: Chalkboard 2.
                    {
                        var text = packet.ReadString();

                        client.Character.Chalkboard = text;
                    }
                    break;

                case 5300000: // NOTE: Fungus Scrol.
                case 5300001: // NOTE: Oinker Delight.
                case 5300002: // NOTE: Zeta Nightmare.
                    {

                    }
                    break;

                case 5090000: // NOTE: Note (Memo).
                    {
                        var targetName = packet.ReadString();
                        var message = packet.ReadString();
                        if (client.Character.Memos.Create(targetName, message))
                        {
                            used = true;
                        }
                    }
                    break;

                case 5100000: // NOTE: Congratulatory Song.
                    {

                    }
                    break;
            }

            if (used)
            {
                client.Character.Items.Remove(itemId, 1);
            }
            else
            {
                client.Character.Release(); // TODO: Blank inventory update.
            }
        }
    }
}