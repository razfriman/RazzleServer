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
            List<string> listEntries = new List<string>();
            byte[] wzFileBytes = File.ReadAllBytes(filePath);
            WzBinaryReader wzParser = new WzBinaryReader(new MemoryStream(wzFileBytes), WzIv);
            while (wzParser.PeekChar() != -1)
            {
                int len = wzParser.ReadInt32();
                char[] strChrs = new char[len];
                for (int i = 0; i < len; i++)
                    strChrs[i] = (char)wzParser.ReadInt16();
                wzParser.ReadUInt16(); //encrypted null
                string decryptedStr = wzParser.DecryptString(strChrs);
                listEntries.Add(decryptedStr);
            }
            wzParser.Close();
            int lastIndex = listEntries.Count - 1;
            string lastEntry = listEntries[lastIndex];
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "g";
            return listEntries;
        }

        public static void SaveToDisk(string path, WzMapleVersion version, List<string> listEntries)
        {
            SaveToDisk(path, WzTool.GetIvByMapleVersion(version), listEntries);
        }

        public static void SaveToDisk(string path, byte[] WzIv, List<string> listEntries)
        {
            int lastIndex = listEntries.Count - 1;
            string lastEntry = listEntries[lastIndex];
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
            WzBinaryWriter wzWriter = new WzBinaryWriter(File.Create(path), WzIv);
            string s;
            for (int i = 0; i < listEntries.Count; i++)
            {
                s = listEntries[i];
                wzWriter.Write(s.Length);
                char[] encryptedChars = wzWriter.EncryptString(s + (char)0);
                for (int j = 0; j < encryptedChars.Length; j++)
                    wzWriter.Write((short)encryptedChars[j]);
            }
            listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
        }
    }
}