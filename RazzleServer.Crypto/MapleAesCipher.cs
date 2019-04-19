using System;
using System.Security.Cryptography;

namespace RazzleServer.Crypto
{
    public class MapleAesCipher
    {
        private ICryptoTransform AesTransformer { get; }

        /// <summary>
        /// IV to use in the Maple AES section
        /// </summary>
        /// <value>The real iv.</value>
        private byte[] RealIv { get; } = new byte[sizeof(int) * 4];

        public MapleAesCipher(ulong aesKey)
        {
            AesTransformer = new RijndaelManaged
            {
                Key = ExpandKey(aesKey), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7
            }.CreateEncryptor();
        }

        /// <summary>
        /// Performs Maplestory's AES algorithm
        /// </summary>
        public void AesTransform(Span<byte> buffer, Span<byte> ivBytes)
        {
            int remaining = buffer.Length,
                length = 0x5B0,
                start = 0;

            RealIv.AsSpan().Fill(0);

            while (remaining > 0)
            {
                int index;
                for (index = 0; index < RealIv.Length; ++index)
                {
                    RealIv[index] = ivBytes[index % 4];
                }

                if (remaining < length)
                {
                    length = remaining;
                }

                for (index = start; index < start + length; ++index)
                {
                    if ((index - start) % RealIv.Length == 0)
                    {
                        var tempIv = new byte[RealIv.Length];
                        AesTransformer.TransformBlock(RealIv, 0, RealIv.Length, tempIv, 0);
                        tempIv.CopyTo(RealIv.AsSpan());
                    }

                    buffer[index] ^= RealIv[(index - start) % RealIv.Length];
                }

                start += length;
                remaining -= length;
                length = 0x5B4;
            }
        }

        /// <summary>
        /// Expands the key we store as long
        /// </summary>
        /// <returns>The expanded key</returns>
        private static byte[] ExpandKey(ulong aesKey)
        {
            var expand = BitConverter.GetBytes(aesKey);
            var key = new byte[expand.Length * 4];
            for (var i = 0; i < expand.Length; i++)
            {
                key[i * 4] = expand[i];
            }

            return key;
        }
    }
}
