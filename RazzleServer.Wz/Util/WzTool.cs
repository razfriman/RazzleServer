using System;
using RazzleServer.Crypto;

namespace RazzleServer.Wz.Util
{
    public static class WzTool
    {
        public static uint RotateLeft(uint x, byte n) => (x << n) | (x >> (32 - n));

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
            new WzMutableKey(wzIv, CryptoConstants.TrimmedUserKey);
    }
}
