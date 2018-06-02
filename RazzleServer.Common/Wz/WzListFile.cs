using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz
{
    /// <summary>
    /// A class that parses and contains the data of a wz list file
    /// </summary>
    public static class ListFileParser
    {
        /// <summary>
        /// Parses a wz list file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="version"></param>
        public static List<string> ParseListFile(string filePath, WzMapleVersionType version)
        {
            return ParseListFile(filePath, WzTool.GetIvByMapleVersion(version));
        }

        /// <summary>
        /// Parses a wz list file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="WzIv"></param>
        public static List<string> ParseListFile(string filePath, byte[] WzIv)
        {
            var listEntries = new List<string>();
            var wzFileBytes = File.ReadAllBytes(filePath);
            var wzParser = new WzBinaryReader(new MemoryStream(wzFileBytes), WzIv);
            while (wzParser.PeekChar() != -1)
            {
                var len = wzParser.ReadInt32();
                var strChrs = new char[len];
                for (var i = 0; i < len; i++)
                {
                    strChrs[i] = (char)wzParser.ReadInt16();
                }

                wzParser.ReadUInt16(); //encrypted null
                var decryptedStr = wzParser.DecryptString(strChrs);
                listEntries.Add(decryptedStr);
            }
            wzParser.Close();
            var lastIndex = listEntries.Count - 1;
            var lastEntry = listEntries[lastIndex];
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "g";
            return listEntries;
        }

        public static void SaveToDisk(string path, WzMapleVersionType version, List<string> listEntries)
        {
            SaveToDisk(path, WzTool.GetIvByMapleVersion(version), listEntries);
        }

        public static void SaveToDisk(string path, byte[] WzIv, List<string> listEntries)
        {
            var lastIndex = listEntries.Count - 1;
            var lastEntry = listEntries[lastIndex];
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
            var wzWriter = new WzBinaryWriter(File.Create(path), WzIv);
            foreach (var entry in listEntries)
            {
                wzWriter.Write(entry.Length);
                var encryptedChars = wzWriter.EncryptString(entry + (char)0);
                foreach (var encryptedChar in encryptedChars)
                {
                    wzWriter.Write((short)encryptedChar);
                }
            }
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
        }
    }
}