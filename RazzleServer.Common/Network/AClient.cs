using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Server;
using RazzleServer.Common.Util;
using Serilog;

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
        public abstract ILogger Logger { get; }

        protected AClient(Socket session, bool toClient = true)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AesKey, toClient);
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
        }

        public virtual void Disconnected() => Connected = false;

        public abstract void Receive(PacketReader packet);

        public void Send(PacketWriter packet) => Send(packet.ToArray());

        public void Send(byte[] packet)
        {
            if (ServerConfig.Instance.PrintPackets)
            {
                Logger.Information($"Sending: {packet.ByteArrayToString()}");
            }

            Socket?.Send(packet).GetAwaiter().GetResult();
        }

        public void Terminate(string message = null)
        {
            Logger.Information($"Disconnecting Client - {Key}. Reason: {message}");
            Socket.Disconnect();
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
