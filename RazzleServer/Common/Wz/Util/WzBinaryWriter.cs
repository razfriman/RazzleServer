using System.Collections;
using System.IO;
using RazzleServer.Common.MapleCryptoLib;

namespace RazzleServer.Common.WzLib.Util
{
    /*
	   TODO : Maybe WzBinaryReader/Writer should read and contain the hash (this is probably what's going to happen)
	*/
    public class WzBinaryWriter : BinaryWriter
    {
        public WzMutableKey WzKey { get; set; }

        public uint Hash { get; set; }

        public Hashtable StringCache { get; set; }

        public WzHeader Header { get; set; }

        public bool LeaveOpen { get; internal set; }

        public WzBinaryWriter(Stream output, byte[] WzIv)
            : this(output, WzIv, false) { }

        public WzBinaryWriter(Stream output, byte[] WzIv, bool leaveOpen)
            : base(output)
        {
            WzKey = WzKeyGenerator.GenerateWzKey(WzIv);
            StringCache = new Hashtable();
            this.LeaveOpen = leaveOpen;
        }

        public void WriteStringValue(string s, int withoutOffset, int withOffset)
        {
            if (s.Length > 4 && StringCache.ContainsKey(s))
            {
                Write((byte)withOffset);
                Write((int)StringCache[s]);
            }
            else
            {
                Write((byte)withoutOffset);
                int sOffset = (int)this.BaseStream.Position;
                Write(s);
                if (!StringCache.ContainsKey(s))
                {
                    StringCache[s] = sOffset;
                }
            }
        }

        public void WriteWzObjectValue(string s, byte type)
        {
            string storeName = type + "_" + s;
            if (s.Length > 4 && StringCache.ContainsKey(storeName))
            {
                Write((byte)2);
                Write((int)StringCache[storeName]);
            }
            else
            {
                int sOffset = (int)(this.BaseStream.Position - Header.FStart);
                Write(type);
                Write(s);
                if (!StringCache.ContainsKey(storeName))
                {
                    StringCache[storeName] = sOffset;
                }
            }
        }

        public override void Write(string value)
        {
            if (value.Length == 0)
            {
                Write((byte)0);
            }
            else
            {
                bool unicode = false;
                for (int i = 0; i < value.Length; i++)
                {
                    unicode |= value[i] > sbyte.MaxValue;
                }

                if (unicode)
                {
                    ushort mask = 0xAAAA;

                    if (value.Length >= sbyte.MaxValue) // Bugfix - >= because if value.Length = MaxValue, MaxValue will be written and then treated as a long-length marker
                    {
                        Write(sbyte.MaxValue);
                        Write(value.Length);
                    }
                    else
                    {
                        Write((sbyte)value.Length);
                    }

                    for (int i = 0; i < value.Length; i++)
                    {
                        ushort encryptedChar = value[i];
                        encryptedChar ^= (ushort)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2]);
                        encryptedChar ^= mask;
                        mask++;
                        Write(encryptedChar);
                    }
                }
                else // ASCII
                {
                    byte mask = 0xAA;

                    if (value.Length > sbyte.MaxValue) // Note - no need for >= here because of 2's complement (MinValue == -(MaxValue + 1))
                    {
                        Write(sbyte.MinValue);
                        Write(value.Length);
                    }
                    else
                    {
                        Write((sbyte)(-value.Length));
                    }

                    for (int i = 0; i < value.Length; i++)
                    {
                        byte encryptedChar = (byte)value[i];
                        encryptedChar ^= WzKey[i];
                        encryptedChar ^= mask;
                        mask++;
                        Write(encryptedChar);
                    }
                }
            }
        }

        public void Write(string value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (i < value.Length)
                {
                    Write(value[i]);
                }
                else
                {
                    Write((byte)0);
                }
            }
        }

        public char[] EncryptString(string stringToDecrypt)
        {
            char[] outputChars = new char[stringToDecrypt.Length];
            for (int i = 0; i < stringToDecrypt.Length; i++)
                outputChars[i] = (char)(stringToDecrypt[i] ^ ((char)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2])));
            return outputChars;
        }

        public char[] EncryptNonUnicodeString(string stringToDecrypt)
        {
            char[] outputChars = new char[stringToDecrypt.Length];
            for (int i = 0; i < stringToDecrypt.Length; i++)
                outputChars[i] = (char)(stringToDecrypt[i] ^ WzKey[i]);
            return outputChars;
        }

        public void WriteNullTerminatedString(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                Write((byte)value[i]);
            }
            Write((byte)0);
        }

        public void WriteCompressedInt(int value)
        {
            if (value > sbyte.MaxValue || value <= sbyte.MinValue)
            {
                Write(sbyte.MinValue);
                Write(value);
            }
            else
            {
                Write((sbyte)value);
            }
        }

        public void WriteCompressedLong(long value)
        {
            if (value > sbyte.MaxValue || value <= sbyte.MinValue)
            {
                Write(sbyte.MinValue);
                Write(value);
            }
            else
            {
                Write((sbyte)value);
            }
        }

        public void WriteOffset(uint value)
        {
            uint encOffset = (uint)BaseStream.Position;
            encOffset = (encOffset - Header.FStart) ^ 0xFFFFFFFF;
            encOffset *= Hash;
            encOffset -= CryptoConstants.WZ_OffsetConstant;
            encOffset = RotateLeft(encOffset, (byte)(encOffset & 0x1F));
            uint writeOffset = encOffset ^ (value - (Header.FStart * 2));
            Write(writeOffset);
        }

        private uint RotateLeft(uint x, byte n) => ((x) << (n)) | ((x) >> (32 - (n)));

        private uint RotateRight(uint x, byte n) => ((x) >> (n)) | ((x) << (32 - (n)));
       
        public override void Close()
        {
            if (!LeaveOpen)
            {
                base.Close();
            }
        }
    }
}