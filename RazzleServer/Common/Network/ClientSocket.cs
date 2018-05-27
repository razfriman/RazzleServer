using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Common.Network
{
    public class ClientSocket : IDisposable
    {
        private readonly Socket _socket;
        private readonly byte[] _socketBuffer;
        private readonly object _disposeSync;
        private readonly AClient _client;
        private readonly bool _toClient;
        private bool _disposed;
        private IPEndPoint Endpoint { get; }
        private readonly ILogger _log = LogManager.Log;

        public MapleCipherProvider Crypto { get; private set; }
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
            _disposeSync = new object();
            _client = client;
            _toClient = toClient;

            Crypto = new MapleCipherProvider(currentGameVersion, aesKey);
            Crypto.PacketFinished += data => _client.Receive(new PacketReader(data));
            WaitForData();
        }
        private void WaitForData()
        {
            if (!_disposed)
            {
                var error = SocketError.Success;

                var socketArgs = new SocketAsyncEventArgs
                {
                    SocketFlags = SocketFlags.None
                };

                socketArgs.SetBuffer(_socketBuffer, 0, _socketBuffer.Length);
                socketArgs.Completed += PacketReceived;
                _socket.ReceiveAsync(socketArgs);

                if (error != SocketError.Success)
                {
                    Disconnect();
                }
            }
        }

        private void PacketReceived(object sender, SocketAsyncEventArgs e)
        {
            if (!_disposed)
            {
                var size = e.BytesTransferred;

                if (size == 0 || e.SocketError != SocketError.Success)
                {
                    Disconnect();
                }
                else
                {
                    Crypto.AddData(_socketBuffer, 0, size);
                    WaitForData();
                }
            }
        }

        public void SendRawPacket(byte[] final)
        {   
            if (!_disposed)
            {
                var offset = 0;

                while (offset < final.Length)
                {
                    var sent = _socket.Send(final, offset, final.Length - offset, SocketFlags.None, out var outError);

                    if (sent == 0 || outError != SocketError.Success)
                    {
                        Disconnect();
                        return;
                    }

                    offset += sent;
                }
            }
        }

        public void Send(PacketWriter data) => Send(data.ToArray());

        public void Send(byte[] data)
        {
            if (!_disposed)
            {
                var buffer = data.ToArray();
                Crypto.Encrypt(ref buffer, _toClient);
                SendRawPacket(buffer);
            }
        }

        public void Disconnect()
        {
            _log.LogInformation("Client Disconnected");
            Dispose();
        }

        public void Dispose()
        {
            lock (_disposeSync)
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
}
