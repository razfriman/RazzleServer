using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

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
                    used = client.Character.TeleportRocks.Use(itemId, packet);
                }
                    break;

                case 5050001: // NOTE: 1st Job SP Reset.
                case 5050002: // NOTE: 2nd Job SP Reset.
                case 5050003: // NOTE: 3rd Job SP Reset.
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
                case 2090000:
                case 2090001:
                case 2090002:
                case 2090003:
                case 2090004:
                case 2090005:
                case 2090006:
                case 2090007:
                case 2090008:
                    // weather effect
                    break;
                case 2081000: // NOTE: Megaphone.
                {
                    if (client.Character.Level <= 10)
                    {
                        // NOTE: You can't use a megaphone unless you're over level 10.

                        return;
                    }

                    var text = packet.ReadString();

                    var message = string.Format($"{client.Character.Name} : {text}"); // TODO: Include medal name.

                    // NOTE: In GMS, this sends to everyone on the current channel, not the map (despite the item's description).
                    using (var oPacket = new PacketWriter(ServerOperationCode.Notice))
                    {
                        oPacket.WriteByte((byte)NoticeType.Megaphone);
                        oPacket.WriteString(message);
                        client.Character.Map.Send(oPacket);
                    }

                    used = true;
                }
                    break;
                case 2082000: // NOTE: Super Megaphone.
                {
                    if (client.Character.Level <= 10)
                    {
                        // NOTE: You can't use a megaphone unless you're over level 10.
                        return;
                    }

                    var text = packet.ReadString();
                    var whisper = packet.ReadBool();

                    var message = string.Format($"{client.Character.Name} : {text}"); // TODO: Include medal name.

                    using (var oPacket = new PacketWriter(ServerOperationCode.Notice))
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
                case 2110000: // NOTE: Pet Name Tag.
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

                case 2140000: // NOTE: Bronze Sack of Meso.
                case 2140001: // NOTE: Silver Sack of Meso.
                case 2140002: // NOTE: Gold Sack of Meso.
                {
                    client.Character.Meso += item.Meso;
                    client.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, true, item.Meso));
                    used = true;
                }
                    break;


                case 2150000: // NOTE: Congratulatory Song.
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
