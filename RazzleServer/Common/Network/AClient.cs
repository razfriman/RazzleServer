using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Server;
using RazzleServer.Common.Util;

namespace RazzleServer.Common.Network
{
    public abstract class AClient : IDisposable
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public ClientSocket Socket { get; set; }
        public bool Connected { get; set; }
        public DateTime LastPong { get; set; }
        public string Key { get; set; }

        protected static readonly ILogger Log = LogManager.Log;

        public AClient(Socket session)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AESKey);
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
        }

        public abstract void Receive(PacketReader packet);

        public virtual void Send(PacketWriter packet) {
            if (ServerConfig.Instance.PrintPackets)
            {
                Log.LogInformation($"Sending: {packet.ToPacketString()}");
            }

            if (Socket == null) return;

            Socket.Send(packet);
        }

        public virtual void Terminate(string message = null)
        {
            Log.LogInformation($"Disconnecting Client - {Key}");
            Socket.Disconnect();
        }


        public virtual void Disconnected()
        {
            try
            {
                Connected = false;
                Socket?.Dispose();
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Error while disconnecting. Client [{Key}]");
            }
        }

        public virtual void Register()
        {

        }

        public virtual void Unregister()
        {

        }

       
        public void SendHandshake()
        {
            if (Socket == null) return;

            var sIV = Functions.RandomUInt();
            var rIV = Functions.RandomUInt();

            Socket.Crypto.SetVectors(sIV, rIV);

            var writer = new PacketWriter();
            writer.WriteUShort(0x0E);
            writer.WriteUShort(ServerConfig.Instance.Version);
            writer.WriteMapleString(ServerConfig.Instance.SubVersion.ToString());
            writer.WriteUInt(rIV);
            writer.WriteUInt(sIV);
            writer.WriteByte(ServerConfig.Instance.ServerType);
            Socket.SendRawPacket(writer.ToArray());
        }

        public void Dispose()
        {
        }
    }
}
