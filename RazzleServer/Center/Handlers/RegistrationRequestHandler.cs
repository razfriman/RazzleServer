using System;
namespace RazzleServer.Center.Handlers
{
    public class RegistrationRequestHandler
    {
        public RegistrationRequestHandler()
        {
        }


        //private void Register(PacketReader inPacket)
        //{
        //    ServerType type = (ServerType)inPacket.ReadByte();
        //    string securityCode = inPacket.ReadString();

        //    bool valid = true;

        //    using (PacketReader Packet = new PacketWriter(InteroperabilityOperationCode.RegistrationResponse))
        //    {
        //        if (!Enum.IsDefined(typeof(ServerType), type))
        //        {
        //            Packet.WriteByte((byte)ServerRegsitrationResponse.InvalidType);

        //            valid = false;
        //        }
        //        else if (securityCode != CenterClient.SecurityCode)
        //        {
        //            Packet.WriteByte((byte)ServerRegsitrationResponse.InvalidCode);

        //            valid = false;
        //        }
        //        else
        //        {
        //            switch (type)
        //            {
        //                case ServerType.Login:
        //                    {
        //                        if (WvsCenter.Login != null)
        //                        {
        //                            Packet.WriteByte((byte)ServerRegsitrationResponse.Full);

        //                            valid = false;
        //                        }
        //                        else
        //                        {
        //                            Packet.WriteByte((byte)ServerRegsitrationResponse.Valid);

        //                            WvsCenter.Login = this;
        //                        }
        //                    }
        //                    break;

        //                case ServerType.Channel:
        //                case ServerType.Shop:
        //                    {
        //                        World world = WvsCenter.Worlds.Next(type);

        //                        if (world == null)
        //                        {
        //                            Packet.WriteByte((byte)ServerRegsitrationResponse.Full);

        //                            valid = false;
        //                        }
        //                        else
        //                        {
        //                            World = world;

        //                            switch (type)
        //                            {
        //                                case ServerType.Channel:
        //                                    World.Add(this);
        //                                    break;

        //                                case ServerType.Shop:
        //                                    World.Shop = this;
        //                                    break;
        //                            }

        //                            Packet.WriteByte((byte)ServerRegsitrationResponse.Valid);
        //                            Packet.WriteByte(World.ID);
        //                            Packet.WriteString(World.Name);

        //                            if (type == ServerType.Channel)
        //                            {
        //                                Packet.WriteString(World.TickerMessage);
        //                                Packet.WriteByte(ID);
        //                            }

        //                            Packet.WriteUShort(Port);

        //                            if (type == ServerType.Channel)
        //                            {
        //                                Packet.WriteBool(World.AllowMultiLeveling);
        //                                Packet.WriteInt(World.ExperienceRate);
        //                                Packet.WriteInt(World.QuestExperienceRate);
        //                                Packet.WriteInt(World.PartyQuestExperienceRate);
        //                                Packet.WriteInt(World.MesoRate);
        //                                Packet.WriteInt(World.DropRate);
        //                            }
        //                        }
        //                    }
        //                    break;
        //            }
        //        }

        //        this.Send(Packet);
        //    }

        //    if (valid)
        //    {
        //        Type = type;

        //        switch (type)
        //        {
        //            case ServerType.Login:
        //                {
        //                    byte count = inPacket.ReadByte();

        //                    for (byte b = 0; b < count; b++)
        //                    {
        //                        if (WvsCenter.Worlds.Contains(b))
        //                        {
        //                            continue;
        //                        }

        //                        WvsCenter.Worlds.Add(new World(inPacket));
        //                    }

        //                    foreach (World loopWorld in WvsCenter.Worlds)
        //                    {
        //                        foreach (CenterClient loopChannel in loopWorld)
        //                        {
        //                            using (PacketReader Packet = new PacketWriter(InteroperabilityOperationCode.UpdateChannel))
        //                            {
        //                                Packet.WriteByte(loopChannel.World.ID);
        //                                Packet.WriteBool(true);
        //                                Packet.WriteByte(loopChannel.ID);
        //                                Packet.WriteUShort(loopChannel.Port);
        //                                Packet.WriteInt(loopChannel.Population);

        //                                WvsCenter.Login.Send(Packet);
        //                            }
        //                        }
        //                    }

        //                    Log.LogInformation("Registered Login Server.");
        //                }
        //                break;

        //            case ServerType.Channel:
        //                {
        //                    using (PacketReader Packet = new PacketWriter(InteroperabilityOperationCode.UpdateChannel))
        //                    {
        //                        Packet.WriteByte(World.ID);
        //                        Packet.WriteBool(true);
        //                        Packet.WriteByte(ID);
        //                        Packet.WriteUShort(Port);
        //                        Packet.WriteInt(Population);

        //                        WvsCenter.Login.Send(Packet);
        //                    }

        //                    Log.LogInformation("Registered Channel Server ({0}-{1}).", World.Name, ID);
        //                }
        //                break;

        //            case ServerType.Shop:
        //                {
        //                    Log.LogInformation("Registered Shop Server ({0}).", World.Name);
        //                }
        //                break;
        //        }
        //    }
        //}
    }
}
