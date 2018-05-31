using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Packet;
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
        public ILogger Log { get; protected set; }

        protected AClient(Socket session, bool toClient = true)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AesKey, toClient);
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
            Log = LogManager.LogByName(GetType().FullName);
        }

        public abstract void Receive(PacketReader packet);

        public virtual void Send(PacketWriter packet)
        {
            if (ServerConfig.Instance.PrintPackets)
            {
                Log.LogInformation($"Sending: {packet.ToPacketString()}");
            }

            Socket?.Send(packet);
        }

        public virtual void Send(byte[] packet)
        {
            if (ServerConfig.Instance.PrintPackets)
            {
                Log.LogInformation($"Sending: {packet.ByteArrayToString()}");
            }

            Socket?.Send(packet);
        }

        public virtual void Terminate(string message = null)
        {
            Log.LogInformation($"Disconnecting Client - {Key}");
            Socket.Disconnect();
        }


        public virtual void Disconnected()
        {
            Connected = false;
        }

        public virtual void Register()
        {

        }

        public virtual void Unregister()
        {

        }

        public async Task SendHandshake()
        {
            if (Socket == null)
            {
                return;
            }

            var sIv = Functions.RandomUInt();
            var rIv = Functions.RandomUInt();

            Socket.Crypto.SetVectors(sIv, rIv);

            var writer = new PacketWriter();
            writer.WriteUShort(0x0E);
            writer.WriteUShort(ServerConfig.Instance.Version);
            writer.WriteString(ServerConfig.Instance.SubVersion.ToString());
            writer.WriteUInt(rIv);
            writer.WriteUInt(sIv);
            writer.WriteByte(ServerConfig.Instance.ServerType);
            await Socket.SendRawPacket(writer.ToArray());
        }

        public void Dispose() => Socket?.Dispose();
    }
}
