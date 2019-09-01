using System;
using RazzleServer.Crypto;

namespace RazzleServer.Wz.Util
{
    public static class WzTool
    {
        public static uint RotateLeft(uint x, byte n) => (x << n) | (x >> (32 - n));

        public static byte[] GetIvByMapleVersion(WzMapleVersionType ver)
        {
            return ver switch
            {
                WzMapleVersionType.Ems => CryptoConstants.WzMseaiv,
                WzMapleVersionType.Gms => CryptoConstants.WzGmsiv,
                WzMapleVersionType.Bms => new byte[4],
                WzMapleVersionType.Classic => new byte[4],
                WzMapleVersionType.Generate => new byte[4],
                _ => throw new ArgumentOutOfRangeException(nameof(ver), ver, null)
            };
        }

        public static WzMutableKey GenerateWzKey(byte[] wzIv) =>
            new WzMutableKey(wzIv, CryptoConstants.TrimmedUserKey);
    }
}
