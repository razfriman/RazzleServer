using System;
using System.Collections;
using System.IO;
using RazzleServer.Crypto;

namespace RazzleServer.Wz.Util
{
    public static class WzTool
    {
        public const int WzHeader = 0x31474B50; //PKG1

        public static readonly Hashtable StringCache = new Hashtable();

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
            if (s.Length > 4 && StringCache.ContainsKey(storeName))
            {
                return 5;
            }

            StringCache[storeName] = 1;
            return 1 + GetEncodedStringLength(s);
        }

        public static T StringToEnum<T>(string name)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), name);
            }
            catch
            {
                return default;
            }
        }

        public static byte[] GetIvByMapleVersion(WzMapleVersionType ver)
        {
            switch (ver)
            {
                case WzMapleVersionType.Ems:
                    return CryptoConstants.WzMseaiv;
                case WzMapleVersionType.Gms:
                    return CryptoConstants.WzGmsiv;
                default:
                    return new byte[4];
            }
        }

        private static int GetRecognizedCharacters(string source)
        {
            var result = 0;
            foreach (var c in source)
            {
                if (0x20 <= c && c <= 0x7E)
                {
                    result++;
                }
            }

            return result;
        }

        private static double GetDecryptionSuccessRate(string wzPath, WzMapleVersionType encVersion, ref short? version)
        {
            var wzf = version == null 
                ? new WzFile(wzPath, encVersion)
                : new WzFile(wzPath, (short)version, encVersion);

            using (wzf)
            {

                wzf.ParseWzFile();
                if (version == null)
                {
                    version = wzf.Version;
                }

                var recognizedChars = 0;
                var totalChars = 0;

                foreach (var wzdir in wzf.WzDirectory.WzDirectories)
                {
                    recognizedChars += GetRecognizedCharacters(wzdir.Name);
                    totalChars += wzdir.Name.Length;
                }

                foreach (var wzimg in wzf.WzDirectory.WzImages)
                {
                    recognizedChars += GetRecognizedCharacters(wzimg.Name);
                    totalChars += wzimg.Name.Length;
                }

                return recognizedChars / (double)totalChars;
            }
        }

        public static WzMapleVersionType DetectMapleVersion(string wzFilePath, out short fileVersion)
        {
            fileVersion = 0;

            var mapleVersionSuccessRates = new Hashtable();
            short? version = null;
            mapleVersionSuccessRates.Add(WzMapleVersionType.Gms, GetDecryptionSuccessRate(wzFilePath, WzMapleVersionType.Gms, ref version));
            mapleVersionSuccessRates.Add(WzMapleVersionType.Ems, GetDecryptionSuccessRate(wzFilePath, WzMapleVersionType.Ems, ref version));
            mapleVersionSuccessRates.Add(WzMapleVersionType.Bms, GetDecryptionSuccessRate(wzFilePath, WzMapleVersionType.Bms, ref version));
            
            if (version != null)
            {
                fileVersion = (short) version;
            }

            var mostSuitableVersion = WzMapleVersionType.Gms;
            double maxSuccessRate = 0;

            foreach (DictionaryEntry mapleVersionEntry in mapleVersionSuccessRates)
            {
                if ((double)mapleVersionEntry.Value > maxSuccessRate)
                {
                    mostSuitableVersion = (WzMapleVersionType)mapleVersionEntry.Key;
                    maxSuccessRate = (double)mapleVersionEntry.Value;
                }
            }

            if (maxSuccessRate < 0.7 && File.Exists(Path.Combine(Path.GetDirectoryName(wzFilePath), "ZLZ.dll")))
            {
                return WzMapleVersionType.GetFromZlz;
            }

            return mostSuitableVersion;
        }

        public static bool IsListFile(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                return reader.ReadInt32() != WzHeader;
            }
        }
    }
}
