using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RazzleServer.Common.Util;
using RazzleServer.Crypto;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Net
{
    public class ClientSocket : IDisposable
    {
        private readonly Socket _socket;
        private readonly AClient _client;
        private readonly bool _toClient;
        private bool _disposed;
        private IPEndPoint Endpoint { get; }
        private readonly ILogger _log = Log.ForContext<ClientSocket>();

        public bool Connected => !_disposed;
        public string Host { get; }
        public byte[] HostBytes { get; }
        public ushort Port { get; }

        public Pipe Pipe { get; }

        /// <summary>
        /// Packet crypto, Incoming
        /// </summary>
        private MapleCipher RecvCipher { get; }

        /// <summary>
        /// Packet crypto, Outgoing
        /// </summary>
        private MapleCipher SendCipher { get; }

        /// <summary>
        /// Waiting state
        /// </summary>
        private bool IsWaiting { get; set; }

        private bool ToClient { get; }

        /// <summary>
        /// Callback for when a packet is finished
        /// </summary>
        public delegate void CallPacketFinished(byte[] packet);

        /// <summary>
        /// Event called when a packet has been handled by the crypto
        /// </summary>
        public event CallPacketFinished PacketFinished;

        /// <summary>
        /// Callback for when a handshake is finished
        /// </summary>
        public delegate void CallHandshakeFinished(uint siv, uint riv, short version, string subVersion,
            byte serverType);

        /// <summary>
        /// Event called when a handshake has been handled by the crypto
        /// </summary>
        public event CallHandshakeFinished HandshakeFinished;

        public const int MinimumBufferSize = 512;

        public ClientSocket(Socket socket, AClient client, ushort version, ulong aesKey, bool useAesEncryption,
            bool toClient)
        {
            _socket = socket;
            Endpoint = socket.RemoteEndPoint as IPEndPoint;
            Host = Endpoint?.Address.ToString();
            HostBytes = Endpoint?.Address.GetAddressBytes();
            Port = (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
            _client = client;
            _toClient = toClient;
            PacketFinished += data => _client.Receive(new PacketReader(data));
            RecvCipher = new MapleCipher(version, aesKey, useAesEncryption);
            SendCipher = new MapleCipher(version, aesKey, useAesEncryption);
            IsWaiting = true;
            ToClient = toClient;
            Pipe = new Pipe();
            Task.Factory.StartNew(ListenForData);
        }

        public async Task ListenForData()
        {
            var writing = FillPipeAsync(_socket);
            var reading = ReadPipeAsync();
            await Task.WhenAll(reading, writing);
        }

        private async Task FillPipeAsync(Socket socket)
        {
            while (!_disposed)
            {
                try
                {
                    var memory = Pipe.Writer.GetMemory(MinimumBufferSize);
                    var bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    Pipe.Writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error write packet data to pipe");
                    Disconnect();
                    break;
                }

                var result = await Pipe.Writer.FlushAsync();

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
                    do
                    {
                        var result = await Pipe.Reader.ReadAsync();
                        var buffer = result.Buffer;

                        while (buffer.Length > 0)
                        {
                            var packetLength = GetHeader(buffer);

                            if (buffer.Length < packetLength)
                            {
                                break;
                            }

                            var packetData = buffer.Slice(0, packetLength);

                            Decrypt(packetData.ToArray());

                            var next = buffer.GetPosition(packetLength);
                            buffer = buffer.Slice(next);
                            dataLeft = buffer.Length;
                        }

                        Pipe.Reader.AdvanceTo(buffer.Start, buffer.End);

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    } while (!_disposed && dataLeft > (RecvCipher.Handshaken ? 4 : 2));
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Cannot read packet data from pipe");
                Disconnect();
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
                await SendRawPacket(Encrypt(data, _toClient).ToArray());
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
                Pipe.Reader.Complete();
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            finally
            {
                _client.Disconnected();
            }
        }

        /// <summary>
        /// Sets the Recv and Send Vectors for the ciphers
        /// </summary>
        public void SetVectors(uint siv, uint riv)
        {
            SendCipher.SetIv(siv);
            RecvCipher.SetIv(riv);
        }

        /// <summary>
        /// Gets the packet header from the current packet.
        /// </summary>
        private int GetHeader(ReadOnlySequence<byte> buffer) => RecvCipher
            .Handshaken
            ? 4 + MapleCipher.GetPacketLength(buffer.Slice(0, 4).ToArray())
            : 2 + BitConverter.ToUInt16(buffer.Slice(0, 2).ToArray());

        /// <summary>
        /// Encrypts packet data
        /// </summary>
        public Span<byte> Encrypt(Span<byte> data, bool toClient = false) => SendCipher.Encrypt(data, toClient);

        /// <summary>
        /// Decrypts the packet data
        /// </summary>
        private void Decrypt(Span<byte> data)
        {
            if (!RecvCipher.Handshaken)
            {
                var pr = new BinaryReader(new MemoryStream(MapleCipher.Handshake(data).ToArray(), false),
                    Encoding.ASCII);
                var version = pr.ReadInt16();
                var subVersionLength = pr.ReadInt16();
                var subVersion = new string(pr.ReadChars(subVersionLength));
                var siv = pr.ReadUInt32();
                var riv = pr.ReadUInt32();
                var serverType = pr.ReadByte();
                SendCipher.SetIv(siv);
                RecvCipher.SetIv(riv);
                HandshakeFinished?.Invoke(siv, riv, version, subVersion, serverType);
            }
            else
            {
                if (!RecvCipher.CheckHeader(data, !ToClient))
                {
                    throw new InvalidOperationException($"Packet header mismatch Size:{data.Length}");
                }

                var decrypted = RecvCipher.Decrypt(data);
                if (decrypted.Length == 0)
                {
                    return;
                }

                PacketFinished?.Invoke(decrypted.ToArray());
            }
        }
    }
}
