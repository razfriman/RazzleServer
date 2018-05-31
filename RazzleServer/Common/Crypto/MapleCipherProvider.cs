using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Common.Crypto
{
    /// <summary>
    /// Helper class for Cipher related functionality
    /// </summary>
    public class MapleCipherProvider
    {
        /// <summary>
        /// Packet crypto, Incomming
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

        /// <summary>
        /// Data buffer
        /// </summary>
        private Memory<byte> DataBuffer { get; set; }

        /// <summary>
        /// Current data in buffer
        /// </summary>
        private int AvailableData { get; set; }

        /// <summary>
        /// Amount of data to wait on
        /// </summary>
        private int WaitForData { get; set; }

        private bool ToClient { get; }

        /// <summary>
        /// General locker for adding data
        /// </summary>
        private object _addLocker = new object();

        private ILogger _log = LogManager.Log;

        public MapleCipherProvider(ushort currentGameVersion, ulong aesKey, ushort initialBufferSize = 0x100, bool toClient = true)
        {
            RecvCipher = new MapleCipher(currentGameVersion, aesKey);
            SendCipher = new MapleCipher(currentGameVersion, aesKey);

            DataBuffer = new byte[initialBufferSize];
            AvailableData = 0;
            WaitForData = 0;
            IsWaiting = true;
            ToClient = toClient;
        }

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
        public delegate void CallHandshakeFinished(uint siv, uint riv);

        /// <summary>
        /// Event called when a handshake has been handled by the crypto
        /// </summary>
        public event CallHandshakeFinished HandshakeFinished;

        /// <summary>
        /// Adds data to the buffer to await decryption
        /// </summary>
        public void AddData(Memory<byte> data, int offset, int length)
        {
            lock (_addLocker)
            {
                EnsureCapacity(length + AvailableData);

                var srcSlice = data.Slice(offset, length);
                var dstSlice = DataBuffer.Slice(AvailableData, length);
                srcSlice.CopyTo(dstSlice);

                AvailableData += length;
            }

            if (WaitForData != 0)
            {
                if (WaitForData <= AvailableData)
                {
                    var w = WaitForData - 2;
                    if (RecvCipher.Handshaken)
                    {
                        w -= 2;
                    }

                    WaitForData = 0;
                    WaitMore(w);
                }
            }
            if (IsWaiting)
            {
                Wait();
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
        /// Encrypts packet data
        /// </summary>
        public Span<byte> Encrypt(Span<byte> data, bool toClient = false) => SendCipher.Encrypt(data, toClient);

        /// <summary>
        /// Prevents the buffer being to small
        /// </summary>
        private void EnsureCapacity(int length)
        {
            if (DataBuffer.Length > length)
            {
                return;
            }

            DataBuffer = new byte[length];
        }

        /// <summary>
        /// Checks if there is enough data to read, Or waits if there isn't.
        /// </summary>
        private void Wait()
        {
            if (!IsWaiting)
            {
                IsWaiting = true;
            }

            if (AvailableData >= 4)
            {
                IsWaiting = false;
                GetHeader();
            }
        }

        /// <summary>
        /// Second step of the wait sequence
        /// </summary>
        private void WaitMore(int length)
        {
            var add = RecvCipher.Handshaken ? 4 : 2;

            if (AvailableData < length + add)
            {
                WaitForData = length + add;
                return;
            }

            var data = new byte[length + add].AsMemory();
            DataBuffer.Slice(0, data.Length).CopyTo(data.Slice(0, length));

            var copyLength = DataBuffer.Length - (length + add);
            DataBuffer.Slice(length + add, length).CopyTo(DataBuffer.Slice(0, copyLength));

            AvailableData -= length + add;

            Decrypt(data.ToArray());
        }

        /// <summary>
        /// Decrypts the packet data
        /// </summary>
        private void Decrypt(Span<byte> data)
        {
            if (!RecvCipher.Handshaken)
            {
                var pr = new PacketReader(RecvCipher.Handshake(data));
                var version = pr.ReadShort();
                var subVersion = pr.ReadString();
                var siv = pr.ReadUInt();
                var riv = pr.ReadUInt();
                var serverType = pr.ReadByte();
                SendCipher.SetIv(siv);
                RecvCipher.SetIv(riv);
                HandshakeFinished?.Invoke(siv, riv);
            }
            else
            {
                if (!RecvCipher.CheckHeader(data, !ToClient))
                {
                    throw new InvalidOperationException("Packet header mismatch");
                }

                RecvCipher.Decrypt(data);

                if (data.Length == 0)
                {
                    return;
                }

                PacketFinished?.Invoke(data.ToArray());
            }
            Wait();
        }

        /// <summary>
        /// Gets the packet header from the current packet.
        /// </summary>
        private void GetHeader()
        {
            if (!RecvCipher.Handshaken)
            {
                WaitMore(BitConverter.ToUInt16(DataBuffer.Slice(0, 2).Span));
            }
            else
            {
                var packetLength = RecvCipher.GetPacketLength(DataBuffer.Slice(0, 4).Span);
                WaitMore(packetLength);
            }
        }
    }
}