using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Center.Handlers
{
    public class RegistrationRequestHandler : CenterPacketHandler
    {
        private static readonly ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            var type = (ServerType)packet.ReadByte();
            string securityCode = packet.ReadString();

            bool valid = true;

            using (var Packet = new PacketWriter(InteroperabilityOperationCode.RegistrationResponse))
            {
                if (!Enum.IsDefined(typeof(ServerType), type))
                {
                    Packet.WriteByte((byte)ServerRegistrationResponse.InvalidType);
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
                                    Packet.WriteByte((byte)ServerRegistrationResponse.Full);

                                    valid = false;
                                }
                                else
                                {
                                    Packet.WriteByte((byte)ServerRegistrationResponse.Valid);

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
                                    Packet.WriteByte((byte)ServerRegistrationResponse.Full);

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

                                    Packet.WriteByte((byte)ServerRegistrationResponse.Valid);
                                    Packet.WriteByte(client.World.ID);
                                    Packet.WriteString(client.World.Name);

                                    if (type == ServerType.Channel)
                                    {
                                        Packet.WriteString(client.World.TickerMessage);
                                        Packet.WriteByte(client.ID);
                                    }

                                    Packet.WriteUShort(client.Port);

                                    if (type == ServerType.Channel)
                                    {
                                        Packet.WriteBool(client.World.EnableMultiLeveling);
                                        Packet.WriteInt(client.World.ExperienceRate);
                                        Packet.WriteInt(client.World.QuestExperienceRate);
                                        Packet.WriteInt(client.World.PartyQuestExperienceRate);
                                        Packet.WriteInt(client.World.MesoRate);
                                        Packet.WriteInt(client.World.DropRate);
                                    }
                                }
                            }
                            break;
                    }
                }

                client.Send(Packet);
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