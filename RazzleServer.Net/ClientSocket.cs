using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using RazzleServer.Crypto;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Net
{
    public class ClientSocket : IDisposable
    {
        private readonly Socket _socket;
        private readonly Pipe _pipe;
        private readonly AClient _client;
        private bool _disposed;
        private readonly bool _toClient;

        private IPEndPoint Endpoint { get; }
        private readonly ILogger _log = Log.ForContext<ClientSocket>();

        public bool Connected => !_disposed;
        public string Host { get; }
        public byte[] HostBytes { get; }
        public ushort Port { get; }

        public MapleCipherProvider Cipher { get; }

        public const int MinimumBufferSize = 512;

        public ClientSocket(Socket socket, AClient client, ushort version, ulong aesKey, bool useAesEncryption,
            bool toClient)
        {
            _socket = socket;
            _client = client;
            _toClient = toClient;
            _pipe = new Pipe();

            Endpoint = socket?.RemoteEndPoint as IPEndPoint;
            Host = Endpoint?.Address.ToString();
            HostBytes = Endpoint?.Address.GetAddressBytes();
            Port = (ushort)(((IPEndPoint)socket?.LocalEndPoint)?.Port ?? 0);
            Cipher = new MapleCipherProvider(version, aesKey, useAesEncryption, toClient);
            Cipher.PacketFinished += data => _client.Receive(new PacketReader(data));
            if (socket != null && socket.Connected)
            {
                Task.Factory.StartNew(ListenForData);
            }
        }

        public async Task ListenForData()
        {
            var writing = FillPipeAsync();
            var reading = ReadPipeAsync();
            await Task.WhenAll(reading, writing);
        }

        private async Task FillPipeAsync()
        {
            while (!_disposed)
            {
                try
                {
                    var memory = _pipe.Writer.GetMemory(MinimumBufferSize);
                    var bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    _pipe.Writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error writing packet data to pipe");
                    Disconnect();
                    break;
                }

                var result = await _pipe.Writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

        private async Task ReadPipeAsync()
        {
            try
            {
                while (!_disposed)
                {
                    long dataLeft = 0;
                    ReadResult result;
                    do
                    {
                        result = await _pipe.Reader.ReadAsync();
                        var buffer = result.Buffer;
                        ProcessPackets(ref buffer, ref dataLeft);
                        _pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                    } while (!_disposed && !result.IsCompleted && dataLeft > Cipher.ReceiveHeaderSize);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Cannot read packet data from pipe");
                Disconnect();
            }
        }

        private void ProcessPackets(ref ReadOnlySequence<byte> buffer, ref long dataLeft)
        {
            while (buffer.Length > 0)
            {
                var packetLength = Cipher.GetHeader(buffer);

                if (buffer.Length < packetLength)
                {
                    break;
                }

                var packetData = buffer.Slice(0, packetLength);

                Cipher.Decrypt(packetData.ToArray());

                var next = buffer.GetPosition(packetLength);
                buffer = buffer.Slice(next);
                dataLeft = buffer.Length;
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
            if (!_disposed && Connected)
            {
                try
                {
                    await SendRawPacket(Cipher.Encrypt(data, _toClient).ToArray());
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error sending packet");
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            _log.Information("Client Disconnected");
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
                _pipe.Writer.Complete();
                _pipe.Reader.Complete();
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            finally
            {
                _client.Disconnected();
                _client.Connected = false;
            }
        }
    }
}
