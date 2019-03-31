using RazzleServer.Common.Constants;
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

            switch (item.MapleId)
            {
                case 2170000: // NOTE: Teleport Rock.
                    used = client.Character.TeleportRocks.Use(packet);
                    break;

                case 2180001: // NOTE: 1st Job SP Reset.
                case 2180002: // NOTE: 2nd Job SP Reset.
                case 2180003: // NOTE: 3rd Job SP Reset.
                {
                }
                    break;

                case 2180000: // NOTE: AP Reset.
                {
                    var statDestination = (StatisticType)packet.ReadInt();
                    var statSource = (StatisticType)packet.ReadInt();
                    client.Character.PrimaryStats.AddAbility(statDestination, 1, true);
                    client.Character.PrimaryStats.AddAbility(statSource, -1, true);
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
                {
                    var message = packet.ReadString();
                    client.Character.Map.SendWeatherEffect(item.MapleId, message);
                    used = true;
                    break;
                }

                case 2081000:
                {
                    if (client.Character.PrimaryStats.Level <= 10)
                    {
                        return;
                    }

                    var text = packet.ReadString();
                    var message = $"{client.Character.Name} : {text}";
                    using (var oPacket = new PacketWriter(ServerOperationCode.Notice))
                    {
                        oPacket.WriteByte(NoticeType.Megaphone);
                        oPacket.WriteString(message);
                        client.Character.Map.Send(oPacket);
                    }

                    used = true;
                }
                    break;
                case 2082000:
                {
                    if (client.Character.PrimaryStats.Level <= 10)
                    {
                        return;
                    }

                    var text = packet.ReadString();
                    var whisper = packet.ReadBool();
                    var message = $"{client.Character.Name} : {text}";
                    using (var oPacket = new PacketWriter(ServerOperationCode.Notice))
                    {
                        oPacket.WriteByte(NoticeType.SuperMegaphone);
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
                case 2140000: // NOTE: Bronze Sack of Meso.
                case 2140001: // NOTE: Silver Sack of Meso.
                case 2140002: // NOTE: Gold Sack of Meso.
                {
                    client.Character.PrimaryStats.Meso += item.Meso;
                    client.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, true, item.Meso));
                    used = true;
                    break;
                }

                case 2150000:
                {
                    client.Character.Map.SendJukeboxSong(item.MapleId, client.Character.Name);
                    used = true;
                }
                    break;
            }

            if (used)
            {
                client.Character.Items.Remove(itemId, 1);
            }
            else
            {
                client.Character.Release();
            }
        }
    }
}
