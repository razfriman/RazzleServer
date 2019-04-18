using System;
using System.Buffers;
using System.IO;
using System.Text;
using RazzleServer.Common.Util;

namespace RazzleServer.Crypto
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

        private bool ToClient { get; }

        public MapleCipherProvider(ushort currentGameVersion, ulong aesKey, bool useAesEncryption = true,
            bool toClient = true)
        {
            RecvCipher = new MapleCipher(currentGameVersion, aesKey, useAesEncryption);
            SendCipher = new MapleCipher(currentGameVersion, aesKey, useAesEncryption);
            ToClient = toClient;
        }

        /// <summary>
        /// Callback for when a packet is finished
        /// </summary>
        public delegate void CallPacketFinished(Span<byte> packet);

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
        /// Decrypts the packet data
        /// </summary>
        public void Decrypt(Span<byte> data)
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

                PacketFinished?.Invoke(decrypted);
            }
        }

        public int ReceiveHeaderSize => RecvCipher.Handshaken ? 4 : 2;
        
        /// <summary>
        /// Gets the packet header from the current packet.
        /// </summary>
        public int GetHeader(ReadOnlySequence<byte> buffer) => RecvCipher
            .Handshaken
            ? 4 + MapleCipher.GetPacketLength(buffer.Slice(0,4).ToSpan())
            : 2 + BitConverter.ToUInt16(buffer.Slice(0, 2).ToSpan());
    }
}
