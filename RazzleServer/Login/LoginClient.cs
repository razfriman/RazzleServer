using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Server;
using RazzleServer.Common.Network;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;
using RazzleServer.Common.Util;

namespace RazzleServer.Login
{
    public sealed class LoginClient : AClient
    {
        public byte World { get; internal set; }
        public byte Channel { get; internal set; }
        public LoginAccount Account { get; internal set; }
        public string LastUsername { get; internal set; }
        public string LastPassword { get; internal set; }
        public string[] MacAddresses { get; internal set; }
        public LoginServer Server { get; internal set; }

        public override ILogger Log => LogManager.CreateLogger<LoginClient>();

        public LoginClient(Socket socket, LoginServer server)
            : base(socket)
        {
            Server = server;
        }

        public override void Receive(PacketReader packet)
        {
            var header = ClientOperationCode.Unknown;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadByte();

                    if (Server.PacketHandlers.ContainsKey(header))
                    {
                        if (ServerConfig.Instance.PrintPackets && !Server.IgnorePacketPrintSet.Contains(header))
                        {
                            Log.LogInformation($"Received [{header.ToString()}] {packet.ToPacketString()}");
                        }

                        foreach (var handler in Server.PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {packet.ToPacketString()}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        public override void Disconnected()
        {
            try
            {
                base.Disconnected();
                Server.RemoveClient(this);
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Username}]");
            }
        }

    }
}
