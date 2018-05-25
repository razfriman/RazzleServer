using System;
using System.IO;
using System.Security.Cryptography;

namespace RazzleServer.Common.Wz.Util
{
    public class WzMutableKey
    {
        public WzMutableKey(byte[] WzIv, byte[] AesKey)
        {
            iv = WzIv;
            aesKey = AesKey;
        }

        private static readonly int BatchSize = 4096;
        private readonly byte[] iv;
        private readonly byte[] aesKey;

        private byte[] keys;

        public byte this[int index]
        {
            get
            {
                if (keys == null || keys.Length <= index)
                {
                    EnsureKeySize(index + 1);
                }
                return keys[index];
            }
        }

        public void EnsureKeySize(int size)
        {
            if (keys != null && keys.Length >= size)
            {
                return;
            }

            size = (int)Math.Ceiling(1.0 * size / BatchSize) * BatchSize;
            var newKeys = new byte[size];

            if (BitConverter.ToInt32(iv, 0) == 0)
            {
                keys = newKeys;
                return;
            }

            var startIndex = 0;

            if (keys != null)
            {
                Buffer.BlockCopy(keys, 0, newKeys, 0, keys.Length);
                startIndex = keys.Length;
            }

            var aes = Rijndael.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = aesKey;
            aes.Mode = CipherMode.ECB;
            var ms = new MemoryStream(newKeys, startIndex, newKeys.Length - startIndex, true);
            var s = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

            for (var i = startIndex; i < size; i += 16)
            {
                if (i == 0)
                {
                    var block = new byte[16];
                    for (var j = 0; j < block.Length; j++)
                    {
                        block[j] = iv[j % 4];
                    }
                    s.Write(block, 0, block.Length);
                }
                else
                {
                    s.Write(newKeys, i - 16, 16);
                }
            }

            s.Flush();
            ms.Close();
            keys = newKeys;
        }
    }
}
