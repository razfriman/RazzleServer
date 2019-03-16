using System;
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
        private readonly object _addLocker = new object();

        public MapleCipherProvider(ushort currentGameVersion, ulong aesKey, ushort initialBufferSize = 0x100,
            bool toClient = true)
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
        public delegate void CallHandshakeFinished(uint siv, uint riv, short version, string subVersion,
            byte serverType);

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
                Functions.MemoryCopy(data, offset, DataBuffer, AvailableData, length);
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

            var newBuffer = new byte[length].AsMemory();
            Functions.MemoryCopy(DataBuffer, 0, newBuffer, 0, DataBuffer.Length);
            DataBuffer = newBuffer;
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

            if (AvailableData > 4)
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
            Functions.MemoryCopy(DataBuffer, 0, data, 0, data.Length);
            Functions.MemoryCopy(DataBuffer, length + add, DataBuffer, 0, DataBuffer.Length - (length + add));
            AvailableData -= length + add;

            Decrypt(data.Span);
        }

        /// <summary>
        /// Decrypts the packet data
        /// </summary>
        private void Decrypt(Span<byte> data)
        {
            if (!RecvCipher.Handshaken)
            {
                var pr = new PacketReader(MapleCipher.Handshake(data));
                var version = pr.ReadShort();
                var subVersion = pr.ReadString();
                var siv = pr.ReadUInt();
                var riv = pr.ReadUInt();
                var serverType = pr.ReadByte();
                SendCipher.SetIv(siv);
                RecvCipher.SetIv(riv);
                HandshakeFinished?.Invoke(siv, riv, version, subVersion, serverType);
            }
            else
            {
                if (!RecvCipher.CheckHeader(data, !ToClient))
                {
                    throw new InvalidOperationException("Packet header mismatch");
                }

                var decrypted = RecvCipher.Decrypt(data);

                if (decrypted.Length == 0)
                {
                    return;
                }

                PacketFinished?.Invoke(decrypted.ToArray());
            }

            Wait();
        }

        /// <summary>
        /// Gets the packet header from the current packet.
        /// </summary>
        private void GetHeader()
        {
            var packetLength = RecvCipher
                .Handshaken
                ? MapleCipher.GetPacketLength(DataBuffer.Slice(0, 4).Span)
                : BitConverter.ToUInt16(DataBuffer.Slice(0, 2).Span);

            WaitMore(packetLength);
        }
    }
}
