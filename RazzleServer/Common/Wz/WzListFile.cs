using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.WzLib.Util;

namespace RazzleServer.Common.WzLib
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
        public static List<string> ParseListFile(string filePath, WzMapleVersion version)
        {
            return ParseListFile(filePath, WzTool.GetIvByMapleVersion(version));
        }

        /// <summary>
		/// Parses a wz list file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
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

        public static void SaveToDisk(string path, WzMapleVersion version, List<string> listEntries)
        {
            SaveToDisk(path, WzTool.GetIvByMapleVersion(version), listEntries);
        }

        public static void SaveToDisk(string path, byte[] WzIv, List<string> listEntries)
        {
            var lastIndex = listEntries.Count - 1;
            var lastEntry = listEntries[lastIndex];
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
            var wzWriter = new WzBinaryWriter(File.Create(path), WzIv);
            string s;
            for (var i = 0; i < listEntries.Count; i++)
            {
                s = listEntries[i];
                wzWriter.Write(s.Length);
                var encryptedChars = wzWriter.EncryptString(s + (char)0);
                for (var j = 0; j < encryptedChars.Length; j++)
                {
                    wzWriter.Write((short)encryptedChars[j]);
                }
            }
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
        }
    }
}