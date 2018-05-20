using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.AdminCommand)]
    public class AdminCommandHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            if (!client.Character.IsMaster)
            {
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
                            client.Character.Buffs.Add(new Skill(5101004), 1);
                        }
                        else
                        {
                            client.Character.Buffs.Remove(5101004);
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
                            using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                            {
                                oPacket.WriteByte(6);
                                oPacket.WriteByte(1);
                                client.Send(oPacket);
                            }
                        }
                    }
                    break;

                case AdminCommandType.Summon:
                    {
                        var mobId = packet.ReadInt();
                        var count = packet.ReadInt();

                        if (DataProvider.Mobs.Contains(mobId))
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
                        client.Character.Experience += amount;
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
                            using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                            {
                                oPacket.WriteByte(6);
                                oPacket.WriteByte(1);

                                client.Send(oPacket);
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
                        // TODO: We have yet to implement map weather.
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

                        if (target != null)
                        {
                            target.Notify(text, NoticeType.Popup);
                        }

                        using (var oPacket = new PacketWriter(ServerOperationCode.AdminResult))
                        {
                            oPacket.WriteByte(29);
                            oPacket.WriteBool(target != null);
                            client.Send(oPacket);
                        }
                    }
                    break;
            }
        }
    }
}