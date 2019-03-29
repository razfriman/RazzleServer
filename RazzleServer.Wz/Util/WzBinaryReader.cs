using System.IO;
using System.Text;
using RazzleServer.Crypto;

namespace RazzleServer.Wz.Util
{
    public class WzBinaryReader : BinaryReader
    {
        public WzMutableKey WzKey { get; set; }

        public uint Hash { get; set; }

        public WzHeader Header { get; set; }

        public WzBinaryReader(Stream input, byte[] wzIv)
            : base(input) => WzKey = WzTool.GenerateWzKey(wzIv);

        public string ReadStringAtOffset(long offset) => ReadStringAtOffset(offset, false);

        public string ReadStringAtOffset(long offset, bool readByte)
        {
            var currentOffset = BaseStream.Position;
            BaseStream.Position = offset;
            if (readByte)
            {
                ReadByte();
            }

            var returnString = ReadString();
            BaseStream.Position = currentOffset;
            return returnString;
        }

        public override string ReadString()
        {
            var smallLength = ReadSByte();

            if (smallLength == 0)
            {
                return string.Empty;
            }

            return smallLength > 0 ? ReadUnicodeString(smallLength) : ReadAsciiString(smallLength);
        }

        private string ReadAsciiString(sbyte smallLength)
        {
            var retString = new StringBuilder();
            byte mask = 0xAA;
            var length = smallLength == sbyte.MinValue ? ReadInt32() : -smallLength;

            for (var i = 0; i < length; i++)
            {
                var encryptedChar = ReadByte();
                encryptedChar ^= mask;
                encryptedChar ^= WzKey[i];
                retString.Append((char)encryptedChar);
                mask++;
            }

            return retString.ToString();
        }

        private string ReadUnicodeString(sbyte smallLength)
        {
            var retString = new StringBuilder();
            ushort mask = 0xAAAA;
            var length = smallLength == sbyte.MaxValue ? ReadInt32() : smallLength;


            for (var i = 0; i < length; i++)
            {
                var encryptedChar = ReadUInt16();
                encryptedChar ^= mask;
                encryptedChar ^= (ushort)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2]);
                retString.Append((char)encryptedChar);
                mask++;
            }

            return retString.ToString();
        }

        /// <summary>
        /// Reads an ASCII string, without decryption
        /// </summary>
        /// <param name="length">Length of bytes to read</param>
        public string ReadString(int length) => Encoding.ASCII.GetString(ReadBytes(length));

        public string ReadNullTerminatedString()
        {
            var retString = new StringBuilder();
            var b = ReadByte();
            while (b != 0)
            {
                retString.Append((char)b);
                b = ReadByte();
            }

            return retString.ToString();
        }

        public int ReadCompressedInt()
        {
            var sb = ReadSByte();
            return sb == sbyte.MinValue ? ReadInt32() : sb;
        }

        public long ReadLong()
        {
            var sb = ReadSByte();
            return sb == sbyte.MinValue ? ReadInt64() : sb;
        }

        public uint ReadOffset()
        {
            var offset = (uint)BaseStream.Position;
            var encryptedOffset = ReadUInt32();
            offset = (offset - Header.FStart) ^ uint.MaxValue;
            offset *= Hash;
            offset -= CryptoConstants.WzOffsetConstant;
            offset = WzTool.RotateLeft(offset, (byte)(offset & 0x1F));
            offset ^= encryptedOffset;
            offset += Header.FStart * 2;
            return offset;
        }

        public string DecryptString(char[] stringToDecrypt)
        {
            var builder = new StringBuilder(stringToDecrypt.Length);
            for (var i = 0; i < stringToDecrypt.Length; i++)
            {
                builder.Append((char)(stringToDecrypt[i] ^ (char)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2])));
            }

            return builder.ToString();
        }

        public string DecryptNonUnicodeString(char[] stringToDecrypt)
        {
            var builder = new StringBuilder(stringToDecrypt.Length);
            for (var i = 0; i < stringToDecrypt.Length; i++)
            {
                builder.Append((char)(stringToDecrypt[i] ^ WzKey[i]));
            }

            return builder.ToString();
        }

        public string ReadStringBlock(uint offset)
        {
            switch (ReadByte())
            {
                case 0:
                case 0x73:
                    return ReadString();
                case 1:
                case 0x1B:
                    return ReadStringAtOffset(offset + ReadInt32());
                default:
                    return "";
            }
        }
    }
}
