using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib
{
	/// <summary>
	/// A class that parses and contains the data of a wz list file
	/// </summary>
	public class WzListFile : AWzObject
	{
		#region Fields
		internal byte[] mWzFileBytes;
		internal List<string> mListEntries = new List<string>();
		internal string mName = "";
		internal byte[] mWzIv;
        internal WzMapleVersion mVersion;
		#endregion

		/// <summary>
		/// Name of the WzListFile
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }
		/// <summary>
		/// The entries in the list wz file
		/// </summary>
		public string[] WzListEntries { get { return mListEntries.ToArray(); } }
		/// <summary>
		/// The WzObjectType of the file
		/// </summary>
		public override WzObjectType ObjectType { get { return WzObjectType.File; } }
		public override AWzObject Parent { get { return null; } internal set { } }
		public override void Dispose()
		{
			mWzFileBytes = null;
			mName = null;
			mListEntries.Clear();
			mListEntries = null;
		}

		/// <summary>
		/// Open a wz list file from a file on the disk
		/// </summary>
		/// <param name="pFilePath">Path to the wz file</param>
		public WzListFile(string pFilePath, WzMapleVersion pVersion)
		{
			mName = Path.GetFileName(pFilePath);
            mWzIv = WzTool.GetIvByMapleVersion(pVersion);
            this.mVersion = pVersion;
            mWzFileBytes = File.ReadAllBytes(pFilePath);
		}
		/// <summary>
		/// Open a wz list file from an array of bytes in the memory
		/// </summary>
		/// <param name="pFileBytes">The wz file in the memory</param>
		public WzListFile(byte[] pFileBytes, byte[] pWzIv)
		{
			mWzFileBytes = pFileBytes;
			mWzIv = pWzIv;
		}

        public WzListFile(WzMapleVersion pVersion, string pName)
        {
            this.mName = pName;
            this.mVersion = pVersion;
            this.mWzIv = WzTool.GetIvByMapleVersion(pVersion);
        }

		/// <summary>
		/// Parses the wz list file
		/// </summary>
		public void ParseWzFile()
		{
			WzBinaryReader wzParser = new WzBinaryReader(new MemoryStream(mWzFileBytes), mWzIv);
			while (wzParser.PeekChar() != -1)
			{
				int Len = wzParser.ReadInt32();
				char[] List = new char[Len];
				for (int i = 0; i < Len; i++)
					List[i] = (char)wzParser.ReadInt16();
				wzParser.ReadUInt16();
				string Decrypted = wzParser.DecryptString(List);
				if (wzParser.PeekChar() == -1)
					if (Decrypted[Decrypted.Length - 1] == '/')
						Decrypted = Decrypted.TrimEnd("/".ToCharArray()) + "g"; // Last char should always be a g (.img)
				mListEntries.Add(Decrypted);
			}
		}
		internal void SaveToDisk(string pPath)
		{
            WzBinaryWriter wzWriter = new WzBinaryWriter(File.Create(pPath), mWzIv);
            foreach (string entry in mListEntries)
            {
                string newEntry = entry + "\0";
                wzWriter.Write(newEntry.Length);
                wzWriter.Write(newEntry, true, true);
            }
		}
	}
}