using System;
using System.Collections.Generic;
using RazzleServer.Crypto;

namespace RazzleServer.Wz.Util
{
    public static class WzTool
    {
        public static readonly HashSet<string> StringCache = new HashSet<string>();

        public static uint RotateLeft(uint x, byte n) => (x << n) | (x >> (32 - n));

        public static uint RotateRight(uint x, byte n) => (x >> n) | (x << (32 - n));

        public static int GetCompressedIntLength(int i)
        {
            if (i > 127 || i < -127)
            {
                return 5;
            }

            return 1;
        }

        public static int GetEncodedStringLength(string s)
        {
            var len = 0;
            if (string.IsNullOrEmpty(s))
            {
                return 1;
            }

            var unicode = false;
            foreach (var c in s)
            {
                unicode |= c > 255;
            }

            if (unicode)
            {
                if (s.Length > 126)
                {
                    len += 5;
                }
                else
                {
                    len += 1;
                }

                len += s.Length * 2;
            }
            else
            {
                if (s.Length > 127)
                {
                    len += 5;
                }
                else
                {
                    len += 1;
                }

                len += s.Length;
            }

            return len;
        }

        public static int GetWzObjectValueLength(string s, byte type)
        {
            var storeName = type + "_" + s;
            if (s.Length > 4 && StringCache.Contains(storeName))
            {
                return 5;
            }

            StringCache.Add(storeName);
            return 1 + GetEncodedStringLength(s);
        }

        public static byte[] GetIvByMapleVersion(WzMapleVersionType ver)
        {
            switch (ver)
            {
                case WzMapleVersionType.Ems:
                    return CryptoConstants.WzMseaiv;
                case WzMapleVersionType.Gms:
                    return CryptoConstants.WzGmsiv;
                case WzMapleVersionType.Bms:
                case WzMapleVersionType.Classic:
                case WzMapleVersionType.Generate:
                    return new byte[4];
                default:
                    throw new ArgumentOutOfRangeException(nameof(ver), ver, null);
            }
        }

        public static WzMutableKey GenerateWzKey(byte[] wzIv) =>
            new WzMutableKey(wzIv, CryptoConstants.GetTrimmedUserKey());
    }
}
