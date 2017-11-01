using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.RegistrationRequest)]
    public class RegistrationRequestHandler : CenterPacketHandler
    {
        private static readonly ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            var type = (ServerType)packet.ReadByte();

            bool valid = true;

            using (var oPacket = new PacketWriter(InteroperabilityOperationCode.RegistrationResponse))
            {
                if (!Enum.IsDefined(typeof(ServerType), type))
                {
                    oPacket.WriteByte((byte)ServerRegistrationResponse.InvalidType);
                    valid = false;
                }
                else
                {
                    switch (type)
                    {
                        case ServerType.Login:
                            {
                                if (client.Server.Login != null)
                                {
                                    oPacket.WriteByte((byte)ServerRegistrationResponse.Full);

                                    valid = false;
                                }
                                else
                                {
                                    oPacket.WriteByte((byte)ServerRegistrationResponse.Valid);

                                    client.Server.Login = client;
                                }
                            }
                            break;

                        case ServerType.Channel:
                        case ServerType.Shop:
                            {
                                World world = client.Server.Worlds.Next(type);

                                if (world == null)
                                {
                                    oPacket.WriteByte((byte)ServerRegistrationResponse.Full);

                                    valid = false;
                                }
                                else
                                {
                                    client.World = world;

                                    switch (type)
                                    {
                                        case ServerType.Channel:
                                            client.World.Add(client);
                                            break;

                                        case ServerType.Shop:
                                            client.World.Shop = client;
                                            break;
                                    }

                                    oPacket.WriteByte((byte)ServerRegistrationResponse.Valid);
                                    oPacket.WriteByte(client.World.ID);
                                    oPacket.WriteString(client.World.Name);

                                    if (type == ServerType.Channel)
                                    {
                                        oPacket.WriteString(client.World.TickerMessage);
                                        oPacket.WriteByte(client.ID);
                                    }

                                    oPacket.WriteUShort(client.Port);

                                    if (type == ServerType.Channel)
                                    {
                                        oPacket.WriteBool(client.World.EnableMultiLeveling);
                                        oPacket.WriteInt(client.World.ExperienceRate);
                                        oPacket.WriteInt(client.World.QuestExperienceRate);
                                        oPacket.WriteInt(client.World.PartyQuestExperienceRate);
                                        oPacket.WriteInt(client.World.MesoRate);
                                        oPacket.WriteInt(client.World.DropRate);
                                    }
                                }
                            }
                            break;
                    }
                }

                client.Send(oPacket);
            }

            if (valid)
            {
                client.Type = type;

                switch (type)
                {
                    case ServerType.Login:
                        {
                            byte count = packet.ReadByte();

                            for (byte b = 0; b < count; b++)
                            {
                                if (client.Server.Worlds.Contains(b))
                                {
                                    continue;
                                }

                                client.Server.Worlds.Add(new World(packet));
                            }

                            foreach (var loopWorld in client.Server.Worlds)
                            {
                                foreach (var loopChannel in loopWorld)
                                {
                                    using (var Packet = new PacketWriter(InteroperabilityOperationCode.UpdateChannel))
                                    {
                                        Packet.WriteByte(loopChannel.World.ID);
                                        Packet.WriteBool(true);
                                        Packet.WriteByte(loopChannel.ID);
                                        Packet.WriteUShort(loopChannel.Port);
                                        Packet.WriteInt(loopChannel.Population);

                                        client.Server.Login.Send(Packet);
                                    }
                                }
                            }

                            Log.LogInformation("Registered Login Server.");
                        }
                        break;

                    case ServerType.Channel:
                        {
                            using (var Packet = new PacketWriter(InteroperabilityOperationCode.UpdateChannel))
                            {
                                Packet.WriteByte(client.World.ID);
                                Packet.WriteBool(true);
                                Packet.WriteByte(client.ID);
                                Packet.WriteUShort(client.Port);
                                Packet.WriteInt(client.Population);

                                client.Server.Login.Send(Packet);
                            }

                            Log.LogInformation($"Registered Channel Server ({client.World.Name}-{client.ID})");
                        }
                        break;

                    case ServerType.Shop:
                        {
                            Log.LogInformation($"Registered Shop Server ({client.World.Name})");
                        }
                        break;
                }
            }
        }
    }
}