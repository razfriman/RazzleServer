using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using RazzleServer.Common.Util;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Net
{
    public abstract class AClient : IDisposable
    {
        public ushort Version { get; }
        public byte SubVersion { get; }
        public byte ServerType { get; }
        public bool PrintPackets { get; }
        public string Host { get; set; }
        public ushort Port { get; set; }
        public ClientSocket Socket { get; set; }
        public bool Connected { get; set; }
        public DateTime LastPong { get; set; }
        public string Key { get; set; }
        public abstract ILogger Logger { get; }

        protected AClient(Socket session, ushort version, byte subVersion, byte serverType, ulong aesKey,
            bool useAesEncryption, bool printPackets, bool toClient)
        {
            Version = version;
            SubVersion = subVersion;
            ServerType = serverType;
            PrintPackets = printPackets;
            Socket = new ClientSocket(session, this, version, aesKey, useAesEncryption, toClient);
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
        }

        public virtual void Disconnected() => Connected = false;

        public abstract void Receive(PacketReader packet);

        public void Send(PacketWriter packet) => Send(packet.ToArray());

        public void Send(byte[] packet)
        {
            if (PrintPackets)
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

            Socket.SetVectors(sIv, rIv);

            var writer = new PacketWriter();
            writer.WriteUShort(0x0E);
            writer.WriteUShort(Version);
            writer.WriteString(SubVersion.ToString());
            writer.WriteUInt(rIv);
            writer.WriteUInt(sIv);
            writer.WriteByte(ServerType);
            await Socket.SendRawPacket(writer.ToArray());
        }

        public void Dispose() => Socket?.Dispose();
    }
}
