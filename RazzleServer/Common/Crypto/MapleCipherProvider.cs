using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Packet;

namespace RazzleServer.Common.MapleCryptoLib
{
    /// <summary>
    /// Helper class for Cipher related functionality
    /// </summary>
    public class MapleCipherProvider
    {
        /// <summary>
		/// Packet crypto, Incomming
		/// </summary>
		private MapleCipher RecvCipher { get; set; }

        /// <summary>
        /// Packet crypto, Outgoing
        /// </summary>
        private MapleCipher SendCipher { get; set; }

        /// <summary>
        /// Waiting state
        /// </summary>
        private bool IsWaiting { get; set; }

        /// <summary>
        /// Data buffer
        /// </summary>
        private byte[] DataBuffer { get; set; }

        /// <summary>
        /// Current data in buffer
        /// </summary>
        private int AvailableData { get; set; }

        /// <summary>
        /// Amount of data to wait on
        /// </summary>
        private int WaitForData { get; set; }

        /// <summary>
        /// General locker for adding data
        /// </summary>
        private object AddLocker = new object();

        private static ILogger Log = LogManager.Log;

        public MapleCipherProvider(ushort currentGameVersion, ulong aesKey, ushort initialBufferSize = 0x100)
        {
            RecvCipher = new MapleCipher(currentGameVersion, aesKey);
            SendCipher = new MapleCipher(currentGameVersion, aesKey);

            DataBuffer = new byte[initialBufferSize];
            AvailableData = 0;
            WaitForData = 0;
            IsWaiting = true;
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
        public delegate void CallHandshakeFinished(uint SIV, uint RIV);

        /// <summary>
        /// Event called when a handshake has been handled by the crypto
        /// </summary>
        public event CallHandshakeFinished HandshakeFinished;

        /// <summary>
        /// Adds data to the buffer to await decryption
        /// </summary>
        public void AddData(byte[] data, int offset, int length)
        {
            lock (AddLocker)
            {
                EnsureCapacity(length + AvailableData);
                Buffer.BlockCopy(data, offset, DataBuffer, AvailableData, length);
                AvailableData += length;
            }
            if (WaitForData != 0)
            {
                if (WaitForData <= AvailableData)
                {
                    int w = WaitForData - 2;
                    if (RecvCipher.Handshaken)
                        w -= 2;

                    WaitForData = 0;
                    WaitMore(w);
                }
            }
            if (IsWaiting)
                Wait();
        }

        /// <summary>
        /// Sets the Recv and Send Vectors for the ciphers
        /// </summary>
        public void SetVectors(uint SIV, uint RIV)
        {
            SendCipher.SetIV(SIV);
            RecvCipher.SetIV(RIV);
        }

        /// <summary>
        /// Encrypts packet data
        /// </summary>
        public ushort? Encrypt(ref byte[] data, bool toClient = false) => SendCipher.Encrypt(ref data, toClient);

        /// <summary>
        /// Prevents the buffer being to small
        /// </summary>
        private void EnsureCapacity(int length)
        {
            if (DataBuffer.Length > length) return; //Return as quikly as posible
            byte[] newBuffer = new byte[DataBuffer.Length + 0x50];
            Buffer.BlockCopy(DataBuffer, 0, newBuffer, 0, DataBuffer.Length);
            DataBuffer = newBuffer;
            EnsureCapacity(length);
        }

        /// <summary>
        /// Checks if there is enough data to read, Or waits if there isn't.
        /// </summary>
        private void Wait()
        {
            if (!IsWaiting)
                IsWaiting = true;

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
            int add = RecvCipher.Handshaken ? 4 : 2;

            if (AvailableData < (length + add))
            {
                WaitForData = length + add;
                return;
            }

            byte[] data;

            data = new byte[length + add];
            Buffer.BlockCopy(DataBuffer, 0, data, 0, data.Length);
            Buffer.BlockCopy(DataBuffer, length + add, DataBuffer, 0, DataBuffer.Length - (length + add));
            AvailableData -= (length + add);

            Decrypt(data.ToArray());
        }

        /// <summary>
        /// Decrypts the packet data
        /// </summary>
        private void Decrypt(byte[] data)
        {
            if (!RecvCipher.Handshaken)
            {
                RecvCipher.Handshake(ref data);
                var pr = new PacketReader(data);
                var version = pr.ReadShort();
                var subVersion = pr.ReadString();
                var siv = pr.ReadUInt();
                var riv = pr.ReadUInt();
                SendCipher.SetIV(siv);
                RecvCipher.SetIV(riv);

                HandshakeFinished?.Invoke(siv, riv);
            }
            else
            {
                RecvCipher.Decrypt(ref data);
                if (data.Length == 0)
                {
                    return;
                }

                PacketFinished?.Invoke(data);
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
                WaitMore(BitConverter.ToUInt16(DataBuffer, 0));
            }
            else
            {
                int packetLength = RecvCipher.GetPacketLength(DataBuffer);
                WaitMore(packetLength);
            }
        }
    }
}