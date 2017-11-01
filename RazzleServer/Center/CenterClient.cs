using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Common.Network;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Center
{
    public sealed class CenterClient : AClient
    {
        public static Dictionary<InteroperabilityOperationCode, List<CenterPacketHandler>> PacketHandlers = new Dictionary<InteroperabilityOperationCode, List<CenterPacketHandler>>();

        public ServerType Type { get; internal set; }
        public World World { get; internal set; }
        public byte ID { get; set; }
        public int Population { get; private set; }
        public CenterServer Server { get; private set; }

        public CenterClient(Socket socket, CenterServer server) : base(socket)
        {
            Server = server;
        }

        public override void Register() => Server.AddClient(this);

        public override void Unregister() => Server.RemoveClient(this);

        public override void Terminate(string message = null)
        {
            switch (Type)
            {
                case ServerType.Login:
                    {
                        Server.Login = null;
                        Log.LogWarning("Unregistered Login Server.");
                    }
                    break;

                case ServerType.Channel:
                    {
                        World.Remove(this);

                        var pw = new PacketWriter(InteroperabilityOperationCode.UpdateChannel);
                        pw.WriteByte(World.ID);
                        pw.WriteBool(false);
                        pw.WriteByte(ID);
                        Server.Login?.Send(pw);

                        Log.LogWarning("Unregistered Channel Server ({0}-{1}).", World.Name, ID);
                    }
                    break;

                case ServerType.Shop:
                    {
                        World.Shop = null;

                        Log.LogWarning("Unregistered Shop Server ({0}).", World.Name);
                        break;
                    }
            }
        }

        private void Migrate(PacketReader inPacket)
        {
            string host = inPacket.ReadString();
            int accountID = inPacket.ReadInt();
            int characterID = inPacket.ReadInt();

            var valid = false;

            if (!Server.Migrations.Contains(host))
            {
                valid = true;
                Server.Migrations.Add(new Migration(host, accountID, characterID));
            }

            var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRegisterResponse);
            outPacket.WriteString(host);
            outPacket.WriteBool(valid);
            Send(outPacket);
        }

        public static void RegisterPacketHandlers()
        {
            var types = Assembly.GetEntryAssembly().GetTypes();

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes()
                                     .OfType<InteroperabilityPacketHandlerAttribute>()
                                     .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<CenterPacketHandler>();
                    }

                    handlerCount++;
                    var handler = (CenterPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.LogDebug($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }

        public override void Receive(PacketReader packet)
        {
            var header = InteroperabilityOperationCode.Unknown;

            try
            {
                if (packet.Available >= 2)
                {
                    header = (InteroperabilityOperationCode)packet.ReadUShort();

                    if (PacketHandlers.ContainsKey(header))
                    {
                        Log.LogInformation($"Received [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }
    }
}
