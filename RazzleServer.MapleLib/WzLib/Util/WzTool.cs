using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using RazzleServer.MapleLib.WzLib.Util;

namespace MapleLib.WzLib.Util
{
	public class WzTool
	{

        public static Dictionary<string, int> StringCache = new Dictionary<string, int>();

        public static uint RotateLeft(uint pValue, byte pBits)
        {
            return (uint)(((pValue) << (pBits)) | ((pValue) >> (32 - (pBits))));
        }
        public static uint RotateRight(uint pValue, byte pBits)
        {
            return (uint)(((pValue) >> (pBits)) | ((pValue) << (32 - (pBits))));
        }

		public static int GetCompressedIntLength(int pInt)
		{
			if (pInt > 127 || pInt < -127)
				return 5;
			return 1;
		}

		public static int GetEncodedStringLength(string pString)
		{
			int len = 0;
			if (string.IsNullOrEmpty(pString))
				return 1;
			bool unicode = false;
			foreach (char c in pString)
				if (c > 255)
					unicode = true;
			if (unicode)
			{
				if (pString.Length > 126)
					len += 5;
				else
					len += 1;
				len += pString.Length * 2;
			}
			else
			{
				if (pString.Length > 127)
					len += 5;
				else
					len += 1;
				len += pString.Length;
			}
			return len;
		}

		public static int GetWzObjectValueLength(string pString, byte pType)
		{
			string storeName = pType + "_" + pString;
			if (pString.Length > 4 && StringCache.ContainsKey(storeName))
			{
				return 5;
			}
			else
			{
				StringCache[storeName] = 1;
				return 1 + GetEncodedStringLength(pString);
			}
		}

		public static T StringToEnum<T>(string pName)
		{
			try
			{
				return (T)Enum.Parse(typeof(T), pName);
			}
			catch
			{
				return default(T);
			}
		}

		public static byte[] GetIvByMapleVersion(WzMapleVersion pVersion)
		{
			switch (pVersion)
			{
				case WzMapleVersion.EMS:
					return CryptoConstants.WZ_MSEAIV;
				case WzMapleVersion.GMS:
					return CryptoConstants.WZ_GMSIV;
				case WzMapleVersion.CLASSIC:
                case WzMapleVersion.BMS:
				default:
					return new byte[4];
			}
		}

        public static byte[] Combine(byte[] pFirst, byte[] pSecond)
        {
            byte[] result = new byte[pFirst.Length + pSecond.Length];
            Array.Copy(pFirst, 0, result, 0, pFirst.Length);
            Array.Copy(pSecond, 0, result, pFirst.Length, pSecond.Length);
            return result;
        }
	}
}