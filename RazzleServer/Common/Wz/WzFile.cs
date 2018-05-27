using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Common.Wz
{
    /// <summary>
    /// A class that contains all the information of a wz file
    /// </summary>
    public class WzFile : WzObject
    {
        public static ILogger Log = LogManager.Log;

        #region Fields
        internal string path;
        internal WzDirectory wzDir;
        internal WzHeader header;
        internal string name = "";
        internal short version;
        internal uint versionHash;
        internal short fileVersion;
        internal WzMapleVersion mapleVersion;
        internal byte[] WzIv;
        #endregion

        /// <summary>
        /// The parsed IWzDir after having called ParseWzDirectory(), this can either be a WzDirectory or a WzListDirectory
        /// </summary>
        public WzDirectory WzDirectory => wzDir;

        /// <summary>
        /// Name of the WzFile
        /// </summary>
        public override string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// The WzObjectType of the file
        /// </summary>
        public override WzObjectType ObjectType => WzObjectType.File;

        /// <summary>
        /// Returns WzDirectory[name]
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>WzDirectory[name]</returns>
        public new WzObject this[string name] => WzDirectory[name];

        public WzHeader Header
        {
            get => header;
            set => header = value;
        }

        public short Version
        {
            get => fileVersion;
            set => fileVersion = value;
        }

        public string FilePath => path;

        public WzMapleVersion MapleVersion
        {
            get => mapleVersion;
            set => mapleVersion = value;
        }

        public override WzObject Parent
        {
            get => null;
            internal set { }
        }

        public override WzFile WzFileParent => this;

        public override void Dispose()
        {
            wzDir?.reader?.Close();
            Header = null;
            path = null;
            name = null;
            WzDirectory?.Dispose();
        }

        public WzFile(short gameVersion, WzMapleVersion version)
        {
            wzDir = new WzDirectory();
            Header = WzHeader.GetDefault();
            fileVersion = gameVersion;
            mapleVersion = version;
            WzIv = WzTool.GetIvByMapleVersion(version);
            wzDir.WzIv = WzIv;
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="version"></param>
        public WzFile(string filePath, WzMapleVersion version)
        {
            name = Path.GetFileName(filePath);
            path = filePath;
            fileVersion = -1;
            mapleVersion = version;
            if (version == WzMapleVersion.GetFromZlz)
            {
                var zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            }
            else
            {
                {
                    WzIv = WzTool.GetIvByMapleVersion(version);
                }
            }
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="gameVersion"></param>
        /// <param name="version"></param>
        public WzFile(string filePath, short gameVersion, WzMapleVersion version)
        {
            name = Path.GetFileName(filePath);
            path = filePath;
            fileVersion = gameVersion;
            mapleVersion = version;
            if (version == WzMapleVersion.GetFromZlz)
            {
                var zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            }
            else
            {
                {
                    WzIv = WzTool.GetIvByMapleVersion(version);
                }
            }
        }

        /// <summary>
        /// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
        /// </summary>
        public void ParseWzFile()
        {
            if (mapleVersion == WzMapleVersion.Generate)
            {
                {
                    throw new InvalidOperationException("Cannot call ParseWzFile() if WZ file type is GENERATE");
                }
            }

            ParseMainWzDirectory();
        }

        public void ParseWzFile(byte[] WzIv)
        {
            if (mapleVersion != WzMapleVersion.Generate)
            {
                {
                    throw new InvalidOperationException(
                    "Cannot call ParseWzFile(byte[] generateKey) if WZ file type is not GENERATE");
                }
            }

            this.WzIv = WzIv;
            ParseMainWzDirectory();
        }

        internal void ParseMainWzDirectory()
        {
            if (path == null)
            {
                Log.LogCritical("Path is null");
                return;
            }

            if (!File.Exists(path))
            {
                var message = $"WZ File does not exist at path: '{path}'";
                Log.LogCritical(message);
                throw new FileNotFoundException(message);
            }

            var reader = new WzBinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), WzIv);

            Header = new WzHeader();
            Header.Ident = reader.ReadString(4);
            Header.FSize = reader.ReadUInt64();
            Header.FStart = reader.ReadUInt32();
            Header.Copyright = reader.ReadNullTerminatedString();
            reader.ReadBytes((int)(Header.FStart - reader.BaseStream.Position));
            reader.Header = Header;
            version = reader.ReadInt16();
            if (fileVersion == -1)
            {
                for (var j = 0; j < short.MaxValue; j++)
                {
                    fileVersion = (short)j;
                    versionHash = GetVersionHash(version, fileVersion);
                    if (versionHash != 0)
                    {
                        reader.Hash = versionHash;
                        var position = reader.BaseStream.Position;
                        WzDirectory testDirectory = null;
                        try
                        {
                            testDirectory = new WzDirectory(reader, name, versionHash, WzIv, this);
                            testDirectory.ParseDirectory();
                        }
                        catch
                        {
                            reader.BaseStream.Position = position;
                            continue;
                        }
                        var testImage = testDirectory.GetChildImages()[0];

                        try
                        {
                            reader.BaseStream.Position = testImage.Offset;
                            var checkByte = reader.ReadByte();
                            reader.BaseStream.Position = position;
                            testDirectory.Dispose();
                            switch (checkByte)
                            {
                                case 0x73:
                                case 0x1b:
                                    {
                                        var directory = new WzDirectory(reader, name, versionHash, WzIv, this);
                                        directory.ParseDirectory();
                                        wzDir = directory;
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

            {
                versionHash = GetVersionHash(version, fileVersion);
                reader.Hash = versionHash;
                var directory = new WzDirectory(reader, name, versionHash, WzIv, this);
                directory.ParseDirectory();
                wzDir = directory;
            }
        }

        private uint GetVersionHash(int encver, int realver)
        {
            var EncryptedVersionNumber = encver;
            var VersionNumber = realver;
            var VersionHash = 0;
            var DecryptedVersionNumber = 0;
            string VersionNumberStr;
            int a = 0, b = 0, c = 0, d = 0, l = 0;

            VersionNumberStr = VersionNumber.ToString();

            l = VersionNumberStr.Length;
            for (var i = 0; i < l; i++)
            {
                VersionHash = 32 * VersionHash + VersionNumberStr[i] + 1;
            }
            a = (VersionHash >> 24) & 0xFF;
            b = (VersionHash >> 16) & 0xFF;
            c = (VersionHash >> 8) & 0xFF;
            d = VersionHash & 0xFF;
            DecryptedVersionNumber = 0xff ^ a ^ b ^ c ^ d;

            return EncryptedVersionNumber == DecryptedVersionNumber 
                ? Convert.ToUInt32(VersionHash) 
                : 0;
        }

        private void CreateVersionHash()
        {
            versionHash = 0;
            foreach (var ch in fileVersion.ToString())
            {
                versionHash = versionHash * 32 + (byte)ch + 1;
            }
            uint a = (versionHash >> 24) & 0xFF,
                b = (versionHash >> 16) & 0xFF,
                c = (versionHash >> 8) & 0xFF,
                d = versionHash & 0xFF;
            version = (byte)~(a ^ b ^ c ^ d);
        }

        /// <summary>
        /// Saves a wz file to the disk, AKA repacking.
        /// </summary>
        /// <param name="path">Path to the output wz file</param>
        public void SaveToDisk(string path)
        {
            WzIv = WzTool.GetIvByMapleVersion(mapleVersion);
            CreateVersionHash();
            wzDir.SetHash(versionHash);
            var tempFile = Path.GetFileNameWithoutExtension(path) + ".TEMP";
            File.Create(tempFile).Close();
            wzDir.GenerateDataFile(tempFile);
            WzTool.StringCache.Clear();
            var totalLen = wzDir.GetImgOffsets(wzDir.GetOffsets(Header.FStart + 2));
            var wzWriter = new WzBinaryWriter(File.Create(path), WzIv);
            wzWriter.Hash = versionHash;
            Header.FSize = totalLen - Header.FStart;
            for (var i = 0; i < 4; i++)
            {
                {
                    wzWriter.Write((byte)Header.Ident[i]);
                }
            }

            wzWriter.Write((long)Header.FSize);
            wzWriter.Write(Header.FStart);
            wzWriter.WriteNullTerminatedString(Header.Copyright);
            var extraHeaderLength = Header.FStart - wzWriter.BaseStream.Position;
            if (extraHeaderLength > 0)
            {
                wzWriter.Write(new byte[(int)extraHeaderLength]);
            }
            wzWriter.Write(version);
            wzWriter.Header = Header;
            wzDir.SaveDirectory(wzWriter);
            wzWriter.StringCache.Clear();
            var fs = File.OpenRead(tempFile);
            wzDir.SaveImages(wzWriter, fs);
            fs.Close();
            File.Delete(tempFile);
            wzWriter.StringCache.Clear();
            wzWriter.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExportXml(string path, bool oneFile)
        {
            if (oneFile)
            {
                var fs = File.Create(path + "/" + name + ".xml");
                var writer = new StreamWriter(fs);

                var level = 0;
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzFile", name, true));
                wzDir.ExportXml(writer, oneFile, level, false);
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzFile"));

                writer.Close();
            }
            else
            {
                throw new Exception("Under Construction");
            }
        }

        /// <summary>
        /// Returns an array of objects from a given path. Wild cards are supported
        /// For example :
        /// GetObjectsFromPath("Map.wz/Map0/*");
        /// Would return all the objects (in this case images) from the sub directory Map0
        /// </summary>
        /// <param name="path">The path to the object(s)</param>
        /// <returns>An array of IWzObjects containing the found objects</returns>
        public List<WzObject> GetObjectsFromWildcardPath(string path)
        {
            if (path.ToLower() == name.ToLower())
            {
                {
                    return new List<WzObject> { WzDirectory };
                }
            }

            if (path == "*")
            {
                var fullList = new List<WzObject>
                {
                    WzDirectory
                };
                fullList.AddRange(GetObjectsFromDirectory(WzDirectory));
                return fullList;
            }

            if (!path.Contains("*"))
            {
                {
                    return new List<WzObject> { GetObjectFromPath(path) };
                }
            }

            var seperatedNames = path.Split("/".ToCharArray());
            if (seperatedNames.Length == 2 && seperatedNames[1] == "*")
            {
                {
                    return GetObjectsFromDirectory(WzDirectory);
                }
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                {
                    foreach (var spath in GetPathsFromImage(img, name + "/" + img.Name))
                    {
                        {
                            if (StrMatch(path, spath))
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var dir in wzDir.WzDirectories)
            {
                {
                    foreach (var spath in GetPathsFromDirectory(dir, name + "/" + dir.Name))
                    {
                        {
                            if (StrMatch(path, spath))
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromRegexPath(string path)
        {
            if (path.ToLower() == name.ToLower())
            {
                {
                    return new List<WzObject> { WzDirectory };
                }
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                {
                    foreach (var spath in GetPathsFromImage(img, name + "/" + img.Name))
                    {
                        {
                            if (Regex.Match(spath, path).Success)
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var dir in wzDir.WzDirectories)
            {
                {
                    foreach (var spath in GetPathsFromDirectory(dir, name + "/" + dir.Name))
                    {
                        {
                            if (Regex.Match(spath, path).Success)
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromDirectory(WzDirectory dir)
        {
            var objList = new List<WzObject>();
            foreach (var img in dir.WzImages)
            {
                objList.Add(img);
                objList.AddRange(GetObjectsFromImage(img));
            }
            foreach (var subdir in dir.WzDirectories)
            {
                objList.Add(subdir);
                objList.AddRange(GetObjectsFromDirectory(subdir));
            }
            return objList;
        }

        public List<WzObject> GetObjectsFromImage(WzImage img)
        {
            var objList = new List<WzObject>();
            foreach (var prop in img.WzProperties)
            {
                objList.Add(prop);
                objList.AddRange(GetObjectsFromProperty(prop));
            }
            return objList;
        }

        public List<WzObject> GetObjectsFromProperty(WzImageProperty prop)
        {
            var objList = new List<WzObject>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    foreach (var canvasProp in ((WzCanvasProperty)prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(canvasProp));
                        }
                    }

                    objList.Add(((WzCanvasProperty)prop).PngProperty);
                    break;
                case WzPropertyType.Convex:
                    foreach (var exProp in ((WzConvexProperty)prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(exProp));
                        }
                    }

                    break;
                case WzPropertyType.SubProperty:
                    foreach (var subProp in ((WzSubProperty)prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(subProp));
                        }
                    }

                    break;
                case WzPropertyType.Vector:
                    objList.Add(((WzVectorProperty)prop).X);
                    objList.Add(((WzVectorProperty)prop).Y);
                    break;
                case WzPropertyType.Null:
                case WzPropertyType.Short:
                case WzPropertyType.Int:
                case WzPropertyType.Long:
                case WzPropertyType.Float:
                case WzPropertyType.Double:
                case WzPropertyType.String:
                case WzPropertyType.Sound:
                case WzPropertyType.UOL:
                case WzPropertyType.PNG:
                    break;
            }
            return objList;
        }

        internal List<string> GetPathsFromDirectory(WzDirectory dir, string curPath)
        {
            var objList = new List<string>();
            foreach (var img in dir.WzImages)
            {
                objList.Add(curPath + "/" + img.Name);

                objList.AddRange(GetPathsFromImage(img, curPath + "/" + img.Name));
            }
            foreach (var subdir in dir.WzDirectories)
            {
                objList.Add(curPath + "/" + subdir.Name);
                objList.AddRange(GetPathsFromDirectory(subdir, curPath + "/" + subdir.Name));
            }
            return objList;
        }

        internal List<string> GetPathsFromImage(WzImage img, string curPath)
        {
            var objList = new List<string>();
            foreach (var prop in img.WzProperties)
            {
                objList.Add(curPath + "/" + prop.Name);
                objList.AddRange(GetPathsFromProperty(prop, curPath + "/" + prop.Name));
            }
            return objList;
        }

        internal List<string> GetPathsFromProperty(WzImageProperty prop, string curPath)
        {
            var objList = new List<string>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    foreach (var canvasProp in ((WzCanvasProperty)prop).WzProperties)
                    {
                        objList.Add(curPath + "/" + canvasProp.Name);
                        objList.AddRange(GetPathsFromProperty(canvasProp, curPath + "/" + canvasProp.Name));
                    }
                    objList.Add(curPath + "/PNG");
                    break;
                case WzPropertyType.Convex:
                    foreach (var exProp in ((WzConvexProperty)prop).WzProperties)
                    {
                        objList.Add(curPath + "/" + exProp.Name);
                        objList.AddRange(GetPathsFromProperty(exProp, curPath + "/" + exProp.Name));
                    }
                    break;
                case WzPropertyType.SubProperty:
                    foreach (var subProp in ((WzSubProperty)prop).WzProperties)
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
            return objList;
        }

        public WzObject GetObjectFromPath(string path)
        {
            var seperatedPath = path.Split("/".ToCharArray());
            if (seperatedPath[0].ToLower() != wzDir.name.ToLower() && seperatedPath[0].ToLower() != wzDir.name.Substring(0, wzDir.name.Length - 3).ToLower())
            {
                {
                    return null;
                }
            }

            if (seperatedPath.Length == 1)
            {
                {
                    return WzDirectory;
                }
            }

            WzObject curObj = WzDirectory;
            for (var i = 1; i < seperatedPath.Length; i++)
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
                        switch (((WzImageProperty)curObj).PropertyType)
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
                                {
                                    return ((WzVectorProperty)curObj).X;
                                }

                                if (seperatedPath[i] == "Y")
                                {
                                    return ((WzVectorProperty)curObj).Y;
                                }

                                return null;

                            default:
                                return null;
                        }
                }
            }

            return curObj;
        }

        internal bool StrMatch(string strWildCard, string strCompare)
        {
            if (strWildCard.Length == 0)
            {
                {
                    return strCompare.Length == 0;
                }
            }

            if (strCompare.Length == 0)
            {
                {
                    return false;
                }
            }

            if (strWildCard[0] == '*' && strWildCard.Length > 1)
            {
                return strCompare.Where((t, index) => StrMatch(strWildCard.Substring(1), strCompare.Substring(index))).Any();
            }
            else if (strWildCard[0] == '*')
            {
                {
                    return true;
                }
            }
            else if (strWildCard[0] == strCompare[0])
            {
                {
                    return StrMatch(strWildCard.Substring(1), strCompare.Substring(1));
                }
            }

            return false;
        }

        public override void Remove()
        {
            Dispose();
        }
    }
}