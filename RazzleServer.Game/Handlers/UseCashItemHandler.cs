using System;
using RazzleServer.Common.Constants;
using RazzleServer.DataProvider;
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
            var item = client.GameCharacter.Items[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidItem);
                return;
            }

            var used = false;

            switch (item.MapleId)
            {
                case 2170000: // NOTE: Teleport Rock.
                    used = UseTeleportRock(client, packet);
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
                    client.GameCharacter.PrimaryStats.AddAbility(statDestination, 1, true);
                    client.GameCharacter.PrimaryStats.AddAbility(statSource, -1, true);
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
                    client.GameCharacter.Map.SendWeatherEffect(item.MapleId, message);
                    used = true;
                    break;
                }

                case 2081000:
                {
                    var text = packet.ReadString();
                    var message = $"{client.GameCharacter.Name} : {text}";
                    using var pw = new PacketWriter(ServerOperationCode.Notice);
                    pw.WriteByte(NoticeType.Megaphone);
                    pw.WriteString(message);
                    client.GameCharacter.Map.Send(pw);
                    used = true;
                }
                    break;
                case 2082000:
                {
                    var text = packet.ReadString();
                    var whisper = packet.ReadBool();
                    var message = $"{client.GameCharacter.Name} : {text}";
                    using var pw = new PacketWriter(ServerOperationCode.Notice);
                    pw.WriteByte(NoticeType.SuperMegaphone);
                    pw.WriteString(message);
                    pw.WriteByte(client.GameCharacter.Client.Server.ChannelId);
                    pw.WriteBool(whisper);
                    client.GameCharacter.Client.Server.World.Send(pw);

                    used = true;
                }
                    break;
                case 2110000: // NOTE: Pet Name Tag.
                {
                    //// TODO: Get the summoned pet.

                    //string name = iPacket.ReadString();

                    //using (var pw = new PacketWriter(ServerOperationCode.PetNameChanged))
                    //{
                    //    pw
                    //        pw.WriteInt(this.client.Character.Id)
                    //        pw.WriteByte() // NOTE: Index.
                    //        pw.WriteString(name)
                    //        pw.WriteByte();

                    //    this.client.Character.Map.Send(pw);
                    //}
                }
                    break;
                case 2140000: // NOTE: Bronze Sack of Meso.
                case 2140001: // NOTE: Silver Sack of Meso.
                case 2140002: // NOTE: Gold Sack of Meso.
                {
                    client.GameCharacter.PrimaryStats.Meso += item.Meso;
                    client.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, true, item.Meso));
                    used = true;
                    break;
                }

                case 2150000:
                {
                    client.GameCharacter.Map.SendJukeboxSong(item.MapleId, client.GameCharacter.Name);
                    used = true;
                }
                    break;
            }

            if (used)
            {
                client.GameCharacter.Items.Remove(itemId, 1);
            }
            else
            {
                client.GameCharacter.Release();
            }
        }

        private bool UseTeleportRock(GameClient client, PacketReader packet)
        {
            var action = (TeleportRockUseAction)packet.ReadByte();
            int destinationMapId;

            switch (action)
            {
                case TeleportRockUseAction.ByMap:
                {
                    var mapId = packet.ReadInt();

                    if (!client.GameCharacter.TeleportRocks.Contains(mapId))
                    {
                        client.GameCharacter.TeleportRocks.SendRockUpdate(TeleportRockResult.AlreadyThere);
                        return false;
                    }

                    destinationMapId = mapId;
                    break;
                }

                case TeleportRockUseAction.ByPlayer:
                {
                    var targetName = packet.ReadString();
                    var target = client.Server.GetCharacterByName(targetName);

                    if (target == null)
                    {
                        client.GameCharacter.TeleportRocks.SendRockUpdate(TeleportRockResult.DifficultToLocate);
                        return false;
                    }

                    destinationMapId = target.Map.MapleId;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (destinationMapId != -1)
            {
                var originMap = CachedData.Maps.Data[client.GameCharacter.Map.MapleId];
                var destinationMap = CachedData.Maps.Data[destinationMapId];

                if (originMap.MapleId == destinationMap.MapleId)
                {
                    client.GameCharacter.TeleportRocks.SendRockUpdate(TeleportRockResult.AlreadyThere);
                    return false;
                }

                if (originMap.FieldLimit.HasFlag(FieldLimitFlags.TeleportItemLimit))
                {
                    client.GameCharacter.TeleportRocks.SendRockUpdate(TeleportRockResult.CannotGo);
                    return false;
                }

                client.GameCharacter.ChangeMap(destinationMapId);
                return true;
            }

            client.GameCharacter.TeleportRocks.SendRockUpdate(TeleportRockResult.CannotGo);
            return false;
        }
    }
}
