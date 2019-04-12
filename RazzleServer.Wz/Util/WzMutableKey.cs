using System;
using System.IO;
using System.Security.Cryptography;

namespace RazzleServer.Wz.Util
{
    public class WzMutableKey
    {
        private const int BatchSize = 4096;

        private readonly byte[] _iv;
        private readonly byte[] _aesKey;
        private byte[] _keys;

        public WzMutableKey(byte[] wzIv, byte[] aesKey)
        {
            _iv = wzIv;
            _aesKey = aesKey;
        }

        public byte this[int index]
        {
            get
            {
                if (_keys == null || _keys.Length <= index)
                {
                    EnsureKeySize(index + 1);
                }

                return _keys[index];
            }
        }

        public void EnsureKeySize(int size)
        {
            if (_keys != null && _keys.Length >= size)
            {
                return;
            }

            size = (int)Math.Ceiling(1.0 * size / BatchSize) * BatchSize;
            var newKeys = new byte[size];

            if (BitConverter.ToInt32(_iv, 0) == 0)
            {
                _keys = newKeys;
                return;
            }

            var startIndex = 0;

            if (_keys != null)
            {
                Buffer.BlockCopy(_keys, 0, newKeys, 0, _keys.Length);
                startIndex = _keys.Length;
            }

            using var aes = Rijndael.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = _aesKey;
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
                        block[j] = _iv[j % 4];
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

            _keys = newKeys;
        }
    }
}
