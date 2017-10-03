using System.IO;
using MapleLib.MapleCryptoLib;

namespace MapleLib.WzLib.Util
{
    public static class WzKeyGenerator
    {
        #region Methods

        public static byte[] GetIvFromZlz(FileStream zlzStream)
        {
            byte[] iv = new byte[4];

            zlzStream.Seek(0x10040, SeekOrigin.Begin);
            zlzStream.Read(iv, 0, 4);
            return iv;
        }

        private static byte[] GetAesKeyFromZlz(FileStream zlzStream)
        {
            byte[] aes = new byte[32];

            zlzStream.Seek(0x10060, SeekOrigin.Begin);
            for (int i = 0; i < 8; i++)
            {
                zlzStream.Read(aes, i * 4, 4);
                zlzStream.Seek(12, SeekOrigin.Current);
            }
            return aes;
        }

        public static WzMutableKey GenerateWzKey(byte[] WzIv)
        {
            return new WzMutableKey(WzIv, CryptoConstants.GetTrimmedUserKey());
        }
        #endregion
    }
}