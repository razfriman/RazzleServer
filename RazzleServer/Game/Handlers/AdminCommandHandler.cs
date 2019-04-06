using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.AdminCommand)]
    [PacketHandler(ClientOperationCode.AdminCommandLog)]
    [PacketHandler(ClientOperationCode.AdminCommandMessage)]
    public class AdminCommandHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            if (!client.Character.IsMaster)
            {
                client.Character.LogCheatWarning(CheatType.ImperonatingGm);
                return;
            }

            var type = (AdminCommandType)packet.ReadByte();

            switch (type)
            {
                case AdminCommandType.Hide:
                {
                    var hide = packet.ReadBool();

                    if (hide)
                    {
                        client.Character.Buffs.Add(new Skill((int)SkillNames.Gm.Hide), 1);
                    }
                    else
                    {
                        client.Character.Buffs.Remove((int)SkillNames.Gm.Hide);
                    }
                }
                    break;

                case AdminCommandType.Send:
                {
                    var name = packet.ReadString();
                    var destinationId = packet.ReadInt();

                    var target = client.Server.GetCharacterByName(name);

                    if (target != null)
                    {
                        target.ChangeMap(destinationId);
                    }
                    else
                    {
                        using (var pw = new PacketWriter(ServerOperationCode.AdminResult))
                        {
                            pw.WriteByte(6);
                            pw.WriteByte(1);
                            client.Send(pw);
                        }
                    }
                }
                    break;

                case AdminCommandType.Summon:
                {
                    var mobId = packet.ReadInt();
                    var count = packet.ReadInt();

                    if (DataProvider.Mobs.Data.ContainsKey(mobId))
                    {
                        for (var i = 0; i < count; i++)
                        {
                            client.Character.Map.Mobs.Add(new Mob(mobId, client.Character.Position));
                        }
                    }
                    else
                    {
                        // TODO: Actual message.
                        client.Character.Notify("invalid mob: " + mobId);
                    }
                }
                    break;

                case AdminCommandType.CreateItem:
                {
                    var itemId = packet.ReadInt();
                    client.Character.Items.Add(new Item(itemId));
                }
                    break;

                case AdminCommandType.DestroyFirstItem:
                {
                    // TODO: What does this do?
                }
                    break;

                case AdminCommandType.GiveExperience:
                {
                    var amount = packet.ReadInt();
                    client.Character.PrimaryStats.Experience += amount;
                }
                    break;

                case AdminCommandType.Ban:
                {
                    var name = packet.ReadString();

                    var target = client.Server.GetCharacterByName(name);

                    if (target != null)
                    {
                        target.Client.Terminate();
                    }
                    else
                    {
                        using (var pw = new PacketWriter(ServerOperationCode.AdminResult))
                        {
                            pw.WriteByte(AdminResultType.InvalidName);
                            pw.WriteByte(1);

                            client.Send(pw);
                        }
                    }
                }
                    break;

                case AdminCommandType.Block:
                {
                    // TODO: Ban.
                }
                    break;

                case AdminCommandType.ShowMessageMap:
                {
                    // TODO: What does this do?
                }
                    break;

                case AdminCommandType.Snow:
                {
                    var seconds = packet.ReadInt();
                    client.Character.Map.SendWeatherEffect(2090000, "", true, seconds * 1000);
                }
                    break;

                case AdminCommandType.VarSetGet:
                {
                    // TODO: This seems useless. Should we implement this?
                }
                    break;

                case AdminCommandType.Warn:
                {
                    var name = packet.ReadString();
                    var text = packet.ReadString();

                    var target = client.Server.GetCharacterByName(name);

                    target?.Notify(text, NoticeType.Popup);

                    using (var pw = new PacketWriter(ServerOperationCode.AdminResult))
                    {
                        pw.WriteByte(29);
                        pw.WriteBool(target != null);
                        client.Send(pw);
                    }
                }
                    break;
                case AdminCommandType.Log:
                    break;
                case AdminCommandType.SetObjectState:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void BanCharacterMessage(GameClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.BanMessage);
            pw.WriteByte(0);
            client.Send(pw);
        }

        public static void InvalidNameMessage(GameClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.InvalidName);
            pw.WriteByte(0x0A);
            client.Send(pw);
        }

        public static void NpcResult(GameClient client)
        {
            //format ; {string} : {string} = {string} 
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.NpcResult);
            pw.WriteString("");
            pw.WriteString("");
            pw.WriteString("");
            client.Send(pw);
        }
    }
}
