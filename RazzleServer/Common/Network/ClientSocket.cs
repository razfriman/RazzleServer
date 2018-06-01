using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Common.Network
{
    public class ClientSocket : IDisposable
    {
        private readonly Socket _socket;
        private readonly Memory<byte> _socketBuffer;
        private readonly AClient _client;
        private readonly bool _toClient;
        private bool _disposed;
        private IPEndPoint Endpoint { get; }
        private readonly ILogger _log = LogManager.Log;

        public MapleCipherProvider Crypto { get; }
        public bool Connected => !_disposed;
        public string Host { get; }
        public byte[] HostBytes { get; }
        public ushort Port { get; }

        public ClientSocket(Socket socket, AClient client, ushort currentGameVersion, ulong aesKey, bool toClient)
        {
            _socket = socket;
            _socketBuffer = new byte[1024];
            Endpoint = socket.RemoteEndPoint as IPEndPoint;
            Host = Endpoint?.Address.ToString();
            HostBytes = Endpoint?.Address.GetAddressBytes();
            Port = (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
            _client = client;
            _toClient = toClient;

            Crypto = new MapleCipherProvider(currentGameVersion, aesKey);
            Crypto.PacketFinished += data => _client.Receive(new PacketReader(data));
            Task.Factory.StartNew(WaitForData);
        }

        private async Task WaitForData()
        {
            while (!_disposed)
            {
                var result = await _socket.ReceiveAsync(_socketBuffer, SocketFlags.None);
                PacketReceived(result);
            }
        }

        private void PacketReceived(int size)
        {
            if (!_disposed)
            {
                if (size == 0)
                {
                    Disconnect();
                }
                else
                {
                    Crypto.AddData(_socketBuffer, 0, size);
                }
            }
        }

        public async Task SendRawPacket(ReadOnlyMemory<byte> final)
        {
            if (!_disposed)
            {
                var offset = 0;

                while (offset < final.Length)
                {
                    var sent = await _socket.SendAsync(final.Slice(offset), SocketFlags.None);

                    if (sent == 0)
                    {
                        Disconnect();
                        return;
                    }

                    offset += sent;
                }
            }
        }

        public async Task Send(byte[] data)
        {
            if (!_disposed)
            {
                await SendRawPacket(Crypto.Encrypt(data, _toClient).ToArray());
            }
        }

        public void Disconnect()
        {
            _log.LogInformation("Client Disconnected");
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            finally
            {
                _client.Disconnected();
            }
        }
    }
}
