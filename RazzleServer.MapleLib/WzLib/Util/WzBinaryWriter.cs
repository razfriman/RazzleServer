using RazzleServer.MapleLib.WzLib.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MapleLib.WzLib.Util
{
	/*
	   TODO : Maybe WzBinaryReader/Writer should read and contain the hash (this is probably what's going to happen)
	*/
	public class WzBinaryWriter : BinaryWriter
	{
		#region Properties
		public byte[] WzKey { get; set; }
		public uint Hash { get; set; }
		public Dictionary<string,int> StringCache { get; set; }
		public WzHeader Header { get; set; }
		public bool LeaveOpen { get; internal set; }
		#endregion

		#region Constructors
		public WzBinaryWriter(Stream pOutput, byte[] pWzIv, bool pLeaveOpen = false)
			: base(pOutput)
		{
			WzKey = WzKeyGenerator.GenerateWzKey(pWzIv);
            StringCache = new Dictionary<string, int>();
			this.LeaveOpen = pLeaveOpen;
		}
		#endregion

		#region Methods
		public void WriteStringValue(string pString, int pWithoutOffset, int pWithOffset)
		{
			if (pString.Length > 4 && StringCache.ContainsKey(pString))
			{
				Write((byte)pWithOffset);
				Write((int)StringCache[pString]);
			}
			else
			{
				Write((byte)pWithoutOffset);
				int sOffset = (int)this.BaseStream.Position;
				Write(pString);
				if (!StringCache.ContainsKey(pString))
				{
					StringCache[pString] = sOffset;
				}
			}
		}

		public void WriteWzObjectValue(string pString, byte pType)
		{
			string storeName = pType + "_" + pString;
			if (pString.Length > 4 && StringCache.ContainsKey(storeName))
			{
				Write((byte)2);
				Write((int)StringCache[storeName]);
			}
			else
			{
				int sOffset = (int)(this.BaseStream.Position - Header.FStart);
				Write(pType);
				Write(pString);
				if (!StringCache.ContainsKey(storeName))
				{
					StringCache[storeName] = sOffset;
				}
			}
		}

		public void Write(string pValue, bool pEncryption = true, bool pUnicode = false)
		{
            if (!pEncryption)
            {
                base.Write(pValue);
                return;
            }
			if (pValue.Length == 0)
			{
				Write((byte)0);
			}
			else
			{
				for (int i = 0; i < pValue.Length; i++)
				{
					if (pValue[i] > sbyte.MaxValue)
					{
						pUnicode = true;
					}
				}

				if (pUnicode)
				{
					ushort mask = 0xAAAA;

					if (pValue.Length > sbyte.MaxValue)
					{
						Write(sbyte.MaxValue);
						Write(pValue.Length);
					}
					else
					{
						Write((sbyte)pValue.Length);
					}

					for (int i = 0; i < pValue.Length; i++)
					{
						ushort encryptedChar = (ushort)pValue[i];
						encryptedChar ^= (ushort)((WzKey[i * 2 + 1] << 8) + WzKey[i * 2]);
						encryptedChar ^= mask;
						mask++;
						Write(encryptedChar);
					}
				}
				else // ASCII
				{
					byte mask = 0xAA;

					if (pValue.Length > sbyte.MaxValue)
					{
						Write(sbyte.MinValue);
						Write(pValue.Length);
					}
					else
					{
						Write((sbyte)(-pValue.Length));
					}

					for (int i = 0; i < pValue.Length; i++)
					{
						byte encryptedChar = (byte)pValue[i];
						encryptedChar ^= WzKey[i];
						encryptedChar ^= mask;
						mask++;
						Write(encryptedChar);
					}
				}
			}
		}

        /// <summary>
        /// No Encryption
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pLength"></param>
		public void Write(string pValue, int pLength)
		{
			for (int i = 0; i < pLength; i++)
			{
				if (i < pValue.Length)
				{
					Write(pValue[i]);
				}
				else
				{
					Write((byte)0);
				}
			}
		}

        /// <summary>
        /// No Encryption
        /// </summary>
        /// <param name="pValue"></param>
		public void WriteNullTerminatedString(string pValue)
		{
			for (int i = 0; i < pValue.Length; i++)
			{
				Write((byte)pValue[i]);
			}
		}

		public void WriteCompressedInt(int pValue)
		{
			if (pValue > sbyte.MaxValue || pValue <= sbyte.MinValue)
			{
				Write(sbyte.MinValue);
				Write(pValue);
			}
			else
			{
				Write((sbyte)pValue);
			}
		}

		public void WriteOffset(uint pValue)
		{
			uint encOffset = (uint)BaseStream.Position;
			encOffset = (encOffset - Header.FStart) ^ 0xFFFFFFFF;
			encOffset *= Hash;
			encOffset -= CryptoConstants.WZ_OffsetConstant;
			encOffset = WzTool.RotateLeft(encOffset, (byte)(encOffset & 0x1F));
			uint writeOffset = encOffset ^ (pValue - (Header.FStart * 2));
			Write(writeOffset);
		}
        
		#endregion
	}
}