using System;

namespace RazzleServer.Crypto
{
    /// <summary>
    /// Cipher class used for encrypting and decrypting maple packet data
    /// </summary>
    public class MapleCipher
    {
        /// <summary>
        /// Vector to use in the MapleCrypto
        /// </summary>
        private InitializationVector MapleIv { get; set; }

        public MapleAesCipher AesCipher { get; }

        /// <summary>
        /// Game version of the current <see cref="MapleCipher"/> instance
        /// </summary>
        public ushort GameVersion { get; }

        /// <summary>
        /// Bool stating if the current instance received its handshake
        /// </summary>
        public bool Handshaken { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="MapleCipher"/>
        /// </summary>
        /// <param name="currentGameVersion">The current MapleStory version</param>
        /// <param name="aesKey">AES key for the current MapleStory version</param>
        public MapleCipher(ushort currentGameVersion, ulong? aesKey)
        {
            Handshaken = false;
            GameVersion = currentGameVersion;

            if (aesKey.HasValue)
            {
                AesCipher = new MapleAesCipher(aesKey.Value);
            }
        }

        /// <summary>
        /// Encrypts packet data
        /// </summary>
        public Span<byte> Encrypt(ReadOnlySpan<byte> data, bool toClient)
        {
            if (!Handshaken || MapleIv == null)
            {
                return null;
            }

            var newData = new byte[data.Length + 4].AsSpan();
            var header = newData.Slice(0, 4);
            var content = newData.Slice(4, data.Length);
            data.CopyTo(content);
            WriteHeader(header, data.Length, toClient);
            EncryptShanda(content);
            AesCipher?.AesTransform(content, MapleIv.Bytes);
            MapleIv.Shuffle();


            return newData;
        }

        /// <summary>
        /// Decrypts a maple packet contained in <paramref name="data"/>
        /// </summary>
        /// <param name="data">Data to decrypt</param>
        public Span<byte> Decrypt(Span<byte> data)
        {
            if (!Handshaken || MapleIv == null)
            {
                return data;
            }

            var header = data.Slice(0, 4);
            var length = GetPacketLength(header);
            var content = data.Slice(4, length);

            AesCipher?.AesTransform(content, MapleIv.Bytes);
            MapleIv.Shuffle();
            DecryptShanda(content);

            return content;
        }

        /// <summary>
        /// Manually sets the vector for the current instance
        /// </summary>
        public void SetIv(uint iv)
        {
            MapleIv = new InitializationVector(iv);
            Handshaken = true;
        }

        /// <summary>
        /// Handles an handshake for the current instance
        /// </summary>
        public static Span<byte> Handshake(Span<byte> data)
        {
            var length = BitConverter.ToUInt16(data.Slice(0, 2));
            return data.Slice(2, length);
        }

        /// <summary>
        /// Creates a packet header
        /// </summary>
        private void WriteHeader(Span<byte> data, int length, bool toClient)
        {
            var a = MapleIv.HiWord;
            a ^= (ushort)(toClient ? -(GameVersion + 1) : GameVersion);
            var b = a;
            b ^= (ushort)length;
            data[0] = (byte)(a & 0xFF);
            data[1] = (byte)((a >> 8) & 0xFF);
            data[2] = (byte)(b & 0xFF);
            data[3] = (byte)((b >> 8) & 0xFF);
        }

        /// <summary>
        /// Gets the length of <paramref name="data"/>
        /// </summary>
        /// <param name="data">Data to check</param>
        /// <returns>Length of <paramref name="data"/></returns>
        public static int GetPacketLength(in ReadOnlySpan<byte> data) =>
            (data[0] + (data[1] << 8)) ^ (data[2] + (data[3] << 8));

        public bool CheckHeader(in ReadOnlySpan<byte> data, bool toClient) => toClient
            ? CheckHeaderToClient(data)
            : CheckHeaderToServer(data);

        public bool CheckHeaderToServer(in ReadOnlySpan<byte> data)
        {
            var encodedVersion = (ushort)(data[0] + (data[1] << 8));
            var version = (ushort)(encodedVersion ^ MapleIv.HiWord);
            return version == GameVersion;
        }

        public bool CheckHeaderToClient(in ReadOnlySpan<byte> data)
        {
            var encodedVersion = (ushort)(data[0] + (data[1] << 8));
            var version = (ushort)-((encodedVersion ^ MapleIv.HiWord) + 1);
            return version == GameVersion;
        }

        /// <summary>
        /// Decrypts <paramref name="buffer"/> using the custom MapleStory shanda
        /// </summary>
        private static void DecryptShanda(Span<byte> buffer)
        {
            var length = buffer.Length;
            for (var passes = 0; passes < 3; passes++)
            {
                byte xorKey = 0;
                byte save;
                var len = (byte)(length & 0xFF);
                byte temp;
                int i;
                for (i = length - 1; i >= 0; --i)
                {
                    temp = (byte)(RotateLeft(buffer[i], 3) ^ 0x13);
                    save = temp;
                    temp = RotateRight((byte)((xorKey ^ temp) - len), 4);
                    xorKey = save;
                    buffer[i] = temp;
                    --len;
                }

                xorKey = 0;
                len = (byte)(length & 0xFF);
                for (i = 0; i < length; ++i)
                {
                    temp = RotateLeft((byte)~(buffer[i] - 0x48), len & 0xFF);
                    save = temp;
                    temp = RotateRight((byte)((xorKey ^ temp) - len), 3);
                    xorKey = save;
                    buffer[i] = temp;
                    --len;
                }
            }
        }

        /// <summary>
        /// Encrypts <paramref name="buffer"/> using the custom MapleStory shanda
        /// </summary>
        private static void EncryptShanda(Span<byte> buffer)
        {
            var length = buffer.Length;
            for (var passes = 0; passes < 3; passes++)
            {
                byte xorKey = 0;
                var len = (byte)(length & 0xFF);
                byte temp;
                int i;
                for (i = 0; i < length; i++)
                {
                    temp = (byte)((RotateLeft(buffer[i], 3) + len) ^ xorKey);
                    xorKey = temp;
                    temp = (byte)((~RotateRight(temp, len & 0xFF) & 0xFF) + 0x48);
                    buffer[i] = temp;
                    len--;
                }

                xorKey = 0;
                len = (byte)(length & 0xFF);
                for (i = length - 1; i >= 0; i--)
                {
                    temp = (byte)(xorKey ^ (len + RotateLeft(buffer[i], 4)));
                    xorKey = temp;
                    temp = RotateRight((byte)(temp ^ 0x13), 3);
                    buffer[i] = temp;
                    len--;
                }
            }
        }

        /// <summary>
        /// Bitwise shift left
        /// </summary>
        private static byte RotateLeft(byte b, int count)
        {
            var tmp = b << (count & 7);
            return (byte)(tmp | (tmp >> 8));
        }

        /// <summary>
        /// Bitwise shift right
        /// </summary>
        private static byte RotateRight(byte b, int count)
        {
            var tmp = b << (8 - (count & 7));
            return (byte)(tmp | (tmp >> 8));
        }
    }
}
