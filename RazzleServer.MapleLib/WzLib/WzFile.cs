using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib
{
    /// <summary>
    /// A class that contains all the information of a wz file
    /// </summary>
    public class WzFile : WzDirectory
    {
        #region Fields
        internal string mPath;
        internal WzHeader mHeader;
        internal short mVersion = 0;
        internal uint mVersionHash = 0;
        internal short mFileVersion = 0;
        internal WzMapleVersion mMapleVersion;
        #endregion

        /// <summary>
        /// Name of the WzFile
        /// </summary>
        public override string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        /// <summary>
        /// The WzObjectType of the file
        /// </summary>
        public override WzObjectType ObjectType { get { return WzObjectType.File; } }

        public WzHeader Header { get { return mHeader; } set { mHeader = value; } }

        public short Version { get { return mFileVersion; } }

        public string FilePath { get { return mPath; } }

        public WzMapleVersion MapleVersion { get { return mMapleVersion; } }

        public override AWzObject Parent { get { return null; } internal set { } }

        public override void Dispose()
        {
            Header = null;
            mPath = null;
            mName = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public WzFile(short pGameVersion, WzMapleVersion pVersion)
        {
            this.Header = WzHeader.GetDefault();
            mFileVersion = pGameVersion;
            mMapleVersion = pVersion;
            mWzIv = WzTool.GetIvByMapleVersion(pVersion);
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="pFilePath">Path to the wz file</param>
        public WzFile(string pFilePath, WzMapleVersion pVersion)
        {
            mName = Path.GetFileName(pFilePath);
            mPath = pFilePath;
            mFileVersion = -1;
            mMapleVersion = pVersion;
            if (pVersion == WzMapleVersion.LOAD_FROM_ZLZ)
            {
                FileStream zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(pFilePath), "ZLZ.dll"));
                mWzIv = Util.WzKeyGenerator.GetIvFromZlz(zlzStream);
            }
            else
            {
                mWzIv = WzTool.GetIvByMapleVersion(pVersion);
            }
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="pFilePath">Path to the wz file</param>
        public WzFile(string pFilePath, short pGameVersion, WzMapleVersion pVersion)
        {
            mName = Path.GetFileName(pFilePath);
            mPath = pFilePath;
            mFileVersion = pGameVersion;
            mMapleVersion = pVersion;
            if (pVersion == WzMapleVersion.LOAD_FROM_ZLZ)
            {
                FileStream zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(pFilePath), "ZLZ.dll"));
                mWzIv = Util.WzKeyGenerator.GetIvFromZlz(zlzStream);
            }
            else
            {
                mWzIv = WzTool.GetIvByMapleVersion(pVersion);
            }
        }

        /// <summary>
        /// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
        /// </summary>
        public void ParseWzFile()
        {
            if (mMapleVersion == WzMapleVersion.GENERATE)
                throw new InvalidOperationException("Cannot call ParseWzFile() if WZ file type is GENERATE");
            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ParseWzFile(byte[] pWzIv)
        {
            if (mMapleVersion != WzMapleVersion.GENERATE)
                throw new InvalidOperationException(
                    "Cannot call ParseWzFile(byte[] generateKey) if WZ file type is not GENERATE");
            this.mWzIv = pWzIv;
            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal void ParseMainWzDirectory()
        {
            if (this.mPath == null)
            {
                Console.WriteLine("[Error] Path is null");
                return;
            }

            WzBinaryReader reader = new WzBinaryReader(File.Open(this.mPath, FileMode.Open, FileAccess.Read, FileShare.Read), mWzIv);

            this.Header = new WzHeader();
            this.Header.Ident = reader.ReadString(4);
            this.Header.FSize = reader.ReadUInt64();
            this.Header.FStart = reader.ReadUInt32();
            this.Header.Copyright = reader.ReadNullTerminatedString();
            reader.ReadBytes((int)(Header.FStart - reader.BaseStream.Position));
            reader.Header = this.Header;
            this.mVersion = reader.ReadInt16();
            if (mFileVersion == -1)
            {
                for (int j = 0; j < short.MaxValue; j++)
                {
                    this.mFileVersion = (short)j;
                    this.mVersionHash = GetVersionHash(mVersion, mFileVersion);
                    if (this.mVersionHash != 0)
                    {
                        reader.Hash = this.mVersionHash;
                        long position = reader.BaseStream.Position;
                        WzDirectory testDirectory = null;
                        try
                        {
                            testDirectory = new WzDirectory(reader, this.mName, this.mVersionHash, this.mWzIv);
                            testDirectory.ParseDirectory();
                        }
                        catch
                        {
                            reader.BaseStream.Position = position;
                            continue;
                        }
                        WzImage testImage = testDirectory.GetChildImages()[0];

                        try
                        {
                            reader.BaseStream.Position = testImage.Offset;
                            byte checkByte = reader.ReadByte();
                            reader.BaseStream.Position = position;
                            testDirectory.Dispose();
                            switch (checkByte)
                            {
                                case 0x73:
                                case 0x1b:
                                    {

                                        this.mReader = reader;
                                        this.mHash = mVersionHash;
                                        ParseDirectory();
                                        return;
                                    }
                            }
                            reader.BaseStream.Position = position;
                        }
                        catch
                        {
                            reader.BaseStream.Position = position;
                        }
                    }
                }
                throw new Exception("Error with game version hash : The specified game version is incorrect and WzLib was unable to determine the version itself");
            }
            else
            {
                this.mVersionHash = GetVersionHash(mVersion, mFileVersion);
                reader.Hash = this.mVersionHash;
                this.mReader = reader;
                this.mHash = mVersionHash;
                ParseDirectory();
            }
        }

        private uint GetVersionHash(int pEncVer, int pRealVer)
        {
            int EncryptedVersionNumber = pEncVer;
            int VersionNumber = pRealVer;
            int VersionHash = 0;
            int DecryptedVersionNumber = 0;
            string VersionNumberStr;
            int a = 0, b = 0, c = 0, d = 0, l = 0;

            VersionNumberStr = VersionNumber.ToString();

            l = VersionNumberStr.Length;
            for (int i = 0; i < l; i++)
            {
                VersionHash = (32 * VersionHash) + (int)VersionNumberStr[i] + 1;
            }
            a = (VersionHash >> 24) & 0xFF;
            b = (VersionHash >> 16) & 0xFF;
            c = (VersionHash >> 8) & 0xFF;
            d = VersionHash & 0xFF;
            DecryptedVersionNumber = (0xff ^ a ^ b ^ c ^ d);

            if (EncryptedVersionNumber == DecryptedVersionNumber)
            {
                return Convert.ToUInt32(VersionHash);
            }
            else
            {
                return 0;
            }
        }

        private void CreateVersionHash()
        {
            mVersionHash = 0;
            foreach (char ch in mFileVersion.ToString())
            {
                mVersionHash = (mVersionHash * 32) + (byte)ch + 1;
            }
            uint a = (mVersionHash >> 24) & 0xFF,
                b = (mVersionHash >> 16) & 0xFF,
                c = (mVersionHash >> 8) & 0xFF,
                d = mVersionHash & 0xFF;
            mVersion = (byte)~(a ^ b ^ c ^ d);
        }

        /// <summary>
        /// Saves a wz file to the disk, AKA repacking.
        /// </summary>
        /// <param name="pPath">Path to the output wz file</param>
        public void SaveToDisk(string pPath)
        {
            mWzIv = WzTool.GetIvByMapleVersion(mMapleVersion);
            CreateVersionHash();
            SetHash(mVersionHash);
            string tempFile = Path.GetFileNameWithoutExtension(pPath) + ".TEMP";
            File.Create(tempFile);
            GenerateDataFile(tempFile);
            WzTool.StringCache.Clear();
            uint totalLen = GetImgOffsets(GetOffsets(Header.FStart + 2));
            WzBinaryWriter wzWriter = new WzBinaryWriter(File.Create(pPath), mWzIv);
            wzWriter.Hash = (uint)mVersionHash;
            Header.FSize = totalLen - Header.FStart;
            wzWriter.Write(Header.Ident, false);
            wzWriter.Write((long)Header.FSize);
            wzWriter.Write(Header.FStart);
            wzWriter.WriteNullTerminatedString(Header.Copyright);
            wzWriter.Write(new byte[Header.ExtraBytes]);
            wzWriter.Write(mVersion);
            wzWriter.Header = Header;
            SaveDirectory(wzWriter);
            wzWriter.StringCache.Clear();
            FileStream fs = File.OpenRead(tempFile);
            SaveImages(wzWriter, fs);
            File.Delete(tempFile);
            wzWriter.StringCache.Clear();
        }

        public void ExportXml(string pPath, bool pOneFile)
        {
            if (pOneFile)
            {
                FileStream fs = File.Create(pPath + "/" + this.mName + ".xml");
                StreamWriter writer = new StreamWriter(fs);

                int level = 0;
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzFile", this.mName, true));
                ExportXml(writer, pOneFile, level, false);
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzFile"));
            }
            else
            {
                throw new Exception("Under Construction");
            }
        }

        #region Search Methods

        /// <summary>
        /// Returns an array of objects from a given path. Wild cards are supported
        /// For example :
        /// GetObjectsFromPath("Map.wz/Map0/*");
        /// Would return all the objects (in this case images) from the sub directory Map0
        /// </summary>
        /// <param name="path">The path to the object(s)</param>
        /// <returns>An array of AWzObjects containing the found objects</returns>
        public AWzObject[] GetObjectsFromWildcardPath(string path)
        {
            if (path.ToLower() == mName.ToLower())
                return new AWzObject[] { this };
            else if (path == "*")
            {
                List<AWzObject> fullList = new List<AWzObject>();
                fullList.Add(this);
                fullList.AddRange(GetObjectsFromDirectory(this));
                return fullList.ToArray();
            }
            else if (!path.Contains("*"))
                return new AWzObject[] { GetObjectFromPath(path) };
            string[] seperatedNames = path.Split("/".ToCharArray());
            if (seperatedNames.Length == 2 && seperatedNames[1] == "*")
                return GetObjectsFromDirectory(this);
            List<AWzObject> objList = new List<AWzObject>();
            foreach (WzImage img in WzImages)
                foreach (string spath in GetPathsFromImage(img, mName + "/" + img.Name))
                    if (strMatch(path, spath))
                        objList.Add(GetObjectFromPath(spath));
            foreach (WzDirectory dir in WzDirectories)
                foreach (string spath in GetPathsFromDirectory(dir, mName + "/" + dir.Name))
                    if (strMatch(path, spath))
                        objList.Add(GetObjectFromPath(spath));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList.ToArray();
        }

        public AWzObject[] GetObjectsFromRegexPath(string path)
        {
            if (path.ToLower() == mName.ToLower())
                return new AWzObject[] { this };
            List<AWzObject> objList = new List<AWzObject>();
            foreach (WzImage img in WzImages)
                foreach (string spath in GetPathsFromImage(img, mName + "/" + img.Name))
                    if (Regex.Match(spath, path).Success)
                        objList.Add(GetObjectFromPath(spath));
            foreach (WzDirectory dir in WzDirectories)
                foreach (string spath in GetPathsFromDirectory(dir, mName + "/" + dir.Name))
                    if (Regex.Match(spath, path).Success)
                        objList.Add(GetObjectFromPath(spath));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList.ToArray();
        }

        public AWzObject[] GetObjectsFromDirectory(WzDirectory dir)
        {
            List<AWzObject> objList = new List<AWzObject>();
            foreach (WzImage img in dir.WzImages)
            {
                objList.Add(img);
                objList.AddRange(GetObjectsFromImage(img));
            }
            foreach (WzDirectory subdir in dir.WzDirectories)
            {
                objList.Add(subdir);
                objList.AddRange(GetObjectsFromDirectory(subdir));
            }
            return objList.ToArray();
        }

        public AWzObject[] GetObjectsFromImage(WzImage img)
        {
            List<AWzObject> objList = new List<AWzObject>();
            foreach (AWzImageProperty prop in img.WzProperties)
            {
                objList.Add(prop);
                objList.AddRange(GetObjectsFromProperty(prop));
            }
            return objList.ToArray();
        }

        public AWzObject[] GetObjectsFromProperty(AWzImageProperty prop)
        {
            List<AWzObject> objList = new List<AWzObject>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    objList.AddRange(prop.WzProperties);
                    objList.Add(((WzCanvasProperty)prop).PngProperty);
                    break;
                case WzPropertyType.Convex:
                    objList.AddRange(prop.WzProperties);
                    break;
                case WzPropertyType.SubProperty:
                    objList.AddRange(prop.WzProperties);
                    break;
                case WzPropertyType.Vector:
                    objList.Add(((WzVectorProperty)prop).X);
                    objList.Add(((WzVectorProperty)prop).Y);
                    break;
            }
            return objList.ToArray();
        }

        internal string[] GetPathsFromDirectory(WzDirectory dir, string curPath)
        {
            List<string> objList = new List<string>();
            foreach (WzImage img in dir.WzImages)
            {
                objList.Add(curPath + "/" + img.Name);

                objList.AddRange(GetPathsFromImage(img, curPath + "/" + img.Name));
            }
            foreach (WzDirectory subdir in dir.WzDirectories)
            {
                objList.Add(curPath + "/" + subdir.Name);
                objList.AddRange(GetPathsFromDirectory(subdir, curPath + "/" + subdir.Name));
            }
            return objList.ToArray();
        }

        internal string[] GetPathsFromImage(WzImage img, string curPath)
        {
            List<string> objList = new List<string>();
            foreach (AWzImageProperty prop in img.WzProperties)
            {
                objList.Add(curPath + "/" + prop.Name);
                objList.AddRange(GetPathsFromProperty(prop, curPath + "/" + prop.Name));
            }
            return objList.ToArray();
        }

        internal string[] GetPathsFromProperty(AWzImageProperty prop, string curPath)
        {
            List<string> objList = new List<string>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    foreach (AWzImageProperty canvasProp in prop.WzProperties)
                    {
                        objList.Add(curPath + "/" + canvasProp.Name);
                        objList.AddRange(GetPathsFromProperty(canvasProp, curPath + "/" + canvasProp.Name));
                    }
                    objList.Add(curPath + "/PNG");
                    break;
                case WzPropertyType.Convex:
                    foreach (AWzImageProperty conProp in prop.WzProperties)
                    {
                        objList.Add(curPath + "/" + conProp.Name);
                        objList.AddRange(GetPathsFromProperty(conProp, curPath + "/" + conProp.Name));
                    }
                    break;
                case WzPropertyType.SubProperty:
                    foreach (AWzImageProperty subProp in prop.WzProperties)
                    {
                        objList.Add(curPath + "/" + subProp.Name);
                        objList.AddRange(GetPathsFromProperty(subProp, curPath + "/" + subProp.Name));
                    }
                    break;
                case WzPropertyType.Vector:
                    objList.Add(curPath + "/X");
                    objList.Add(curPath + "/Y");
                    break;
            }
            return objList.ToArray();
        }

        public AWzObject GetObjectFromPath(string path)
        {
            string[] seperatedPath = path.Split("/".ToCharArray());
            if (seperatedPath[0].ToLower() != mName.ToLower())
                return null;
            if (seperatedPath.Length == 1)
                return this;
            AWzObject curObj = this;
            for (int i = 1; i < seperatedPath.Length; i++)
            {
                if (curObj == null)
                {
                    return null;
                }
                switch (curObj.ObjectType)
                {
                    case WzObjectType.Directory:
                        curObj = ((WzDirectory)curObj)[seperatedPath[i]];
                        continue;
                    case WzObjectType.Image:
                        curObj = ((WzImage)curObj)[seperatedPath[i]];
                        continue;
                    case WzObjectType.Property:
                        switch (((AWzImageProperty)curObj).PropertyType)
                        {
                            case WzPropertyType.Canvas:
                                curObj = ((WzCanvasProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.Convex:
                                curObj = ((WzConvexProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.SubProperty:
                                curObj = ((WzSubProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.Vector:
                                if (seperatedPath[i] == "X")
                                    return ((WzVectorProperty)curObj).X;
                                else if (seperatedPath[i] == "Y")
                                    return ((WzVectorProperty)curObj).Y;
                                else
                                    return null;
                            default: // Wut?
                                return null;
                        }
                }
            }
            if (curObj == null)
            {
                return null;
            }
            return curObj;
        }

        internal bool strMatch(string strWildCard, string strCompare)
        {
            if (strWildCard.Length == 0) return strCompare.Length == 0;
            if (strCompare.Length == 0) return false;
            if (strWildCard[0] == '*' && strWildCard.Length > 1)
                for (int index = 0; index < strCompare.Length; index++)
                {
                    if (strMatch(strWildCard.Substring(1), strCompare.Substring(index)))
                        return true;
                }
            else if (strWildCard[0] == '*')
                return true;
            else if (strWildCard[0] == strCompare[0])
                return strMatch(strWildCard.Substring(1), strCompare.Substring(1));
            return false;
        }

        #endregion
    }
}