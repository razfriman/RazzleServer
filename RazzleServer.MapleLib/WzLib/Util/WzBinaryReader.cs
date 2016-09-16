using RazzleServer.MapleLib.WzLib.Util;
using System;
using System.IO;
using System.Text;

namespace MapleLib.WzLib.Util
{
	public class WzBinaryReader : BinaryReader
	{
		#region Properties
		public byte[] WzKey { get; set; }
		public uint Hash { get; set; }
		public WzHeader Header { get; set; }
		#endregion

		#region Constructors
		public WzBinaryReader(Stream pInput, byte[] pWzIv)
			: base(pInput)
		{
			WzKey = WzKeyGenerator.GenerateWzKey(pWzIv);
		}
		#endregion

		#region Methods
		public string ReadStringAtOffset(long pOffset)
		{
			return ReadStringAtOffset(pOffset, false);
		}

		public string ReadStringAtOffset(long pOffset, bool pReadByte)
		{
			long CurrentOffset = BaseStream.Position;
			BaseStream.Position = pOffset;
			if (pReadByte)
			{
				ReadByte();
			}
			string ReturnString = ReadString();
			BaseStream.Position = CurrentOffset;
			return ReturnString;
		}

		public override string ReadString()
		{
			sbyte smallLength = base.ReadSByte();

			if (smallLength == 0)
			{
				return string.Empty;
			}

			int length;
			StringBuilder retString = new StringBuilder();
			if (smallLength > 0) // Unicode
			{
				ushort mask = 0xAAAA;
				if (smallLength == sbyte.MaxValue)
				{
					length = ReadInt32();
				}
				else
				{
					length = (int)smallLength;
				}
				if (length <= 0)
				{
					return string.Empty;
				}

				for (int i = 0; i < length; i++)
				{
					ushort encryptedChar = ReadUInt16();
					encryptedChar ^= mask;
					encryptedChar ^= (ushort)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2]);
					retString.Append((char)encryptedChar);
					mask++;
				}
			}
			else
			{ // ASCII
				byte mask = 0xAA;
				if (smallLength == sbyte.MinValue)
				{
					length = ReadInt32();
				}
				else
				{
					length = (int)(-smallLength);
				}
				if (length <= 0)
				{
					return string.Empty;
				}

				for (int i = 0; i < length; i++)
				{
					byte encryptedChar = ReadByte();
					encryptedChar ^= mask;
					encryptedChar ^= (byte)WzKey[i];
					retString.Append((char)encryptedChar);
					mask++;
				}
			}
			return retString.ToString();
		}

		/// <summary>
		/// Reads an ASCII string, without decryption
		/// </summary>
		/// <param name="filePath">Length of bytes to read</param>
		public string ReadString(int pLength)
		{
			return Encoding.ASCII.GetString(ReadBytes(pLength));
		}

		public string ReadNullTerminatedString()
		{
			StringBuilder retString = new StringBuilder();
			byte b = ReadByte();
			while (b != 0)
			{
				retString.Append((char)b);
				b = ReadByte();
			}
			return retString.ToString();
		}

		public int ReadCompressedInt()
		{
			sbyte sb = base.ReadSByte();
			if (sb == sbyte.MinValue)
			{
				return ReadInt32();
			}
			return sb;
		}

		public uint ReadOffset()
		{
			uint offset = (uint)BaseStream.Position;
			offset = (offset - Header.FStart) ^ uint.MaxValue;
			offset *= Hash;
			offset -= CryptoConstants.WZ_OffsetConstant;
			offset = WzTool.RotateLeft(offset, (byte)(offset & 0x1F));
			uint encryptedOffset = ReadUInt32();
			offset ^= encryptedOffset;
			offset += Header.FStart * 2;
			return offset;
		}

		public string DecryptString(char[] pStringToDecrypt)
		{
			string outputString = "";
			for (int i = 0; i < pStringToDecrypt.Length; i++)
				outputString += (char)(pStringToDecrypt[i] ^ ((char)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2])));
			return outputString;
		}

		public string DecryptNonUnicodeString(char[] pStringToDecrypt)
		{
			string outputString = "";
			for (int i = 0; i < pStringToDecrypt.Length; i++)
				outputString += (char)(pStringToDecrypt[i] ^ WzKey[i]);
			return outputString;
		}

		public string ReadStringBlock(uint pOffset)
		{
			switch (ReadByte())
			{
				case 0:
				case 0x73:
					return ReadString();
				case 1:
				case 0x1B:
					return ReadStringAtOffset(pOffset + ReadInt32());
				default:
					return "";
			}
		}

		#endregion
	}
}