using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RazzleServer.Wz.Util;
using RazzleServer.Wz.WzProperties;
using Serilog;

namespace RazzleServer.Wz
{
    /// <inheritdoc />
    /// <summary>
    /// A class that contains all the information of a wz file
    /// </summary>
    public class WzFile : WzObject
    {
        private readonly ILogger _log = Log.ForContext<WzFile>();

        private uint _versionHash;
        private int _version;
        private byte[] _wzIv;

        public WzDirectory WzDirectory { get; private set; } = new WzDirectory();

        public override WzObjectType ObjectType => WzObjectType.File;

        /// <summary>
        /// Returns WzDirectory[name]
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>WzDirectory[name]</returns>
        public new WzObject this[string name] => WzDirectory[name];

        [JsonIgnore] public WzHeader Header { get; set; } = WzHeader.GetDefault();

        public short Version { get; set; }

        [JsonIgnore] public string FilePath { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WzMapleVersionType MapleVersionType { get; set; }

        public override WzFile WzFileParent => this;

        public override void Dispose()
        {
            Header = null;
            FilePath = null;
            Name = null;
            WzDirectory?.Dispose();
        }

        public WzFile()
        {
        }

        public WzFile(short gameVersion, WzMapleVersionType version)
        {
            Version = gameVersion;
            MapleVersionType = version;
            _wzIv = WzTool.GetIvByMapleVersion(version);
            WzDirectory.WzIv = _wzIv;
        }

        /// <inheritdoc />
        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="version"></param>
        public WzFile(string filePath, WzMapleVersionType version) : this(filePath, -1, version) { }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        /// <param name="gameVersion"></param>
        /// <param name="version"></param>
        public WzFile(string filePath, short gameVersion, WzMapleVersionType version)
        {
            Name = Path.GetFileName(filePath);
            FilePath = filePath;
            Version = gameVersion;
            MapleVersionType = version;
            _wzIv = WzTool.GetIvByMapleVersion(version);
        }

        /// <summary>
        /// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
        /// </summary>
        public void ParseWzFile()
        {
            if (MapleVersionType == WzMapleVersionType.Generate)
            {
                throw new InvalidOperationException("Cannot call ParseWzFile() if WZ file type is GENERATE");
            }

            ParseMainWzDirectory();
        }

        public void ParseWzFile(byte[] wzIv)
        {
            if (MapleVersionType != WzMapleVersionType.Generate)
            {
                throw new InvalidOperationException(
                    "Cannot call ParseWzFile(byte[] generateKey) if WZ file type is not GENERATE");
            }

            _wzIv = wzIv;
            ParseMainWzDirectory();
        }

        internal void ParseMainWzDirectory()
        {
            if (FilePath == null)
            {
                _log.Error("Path is null");
                return;
            }

            if (!File.Exists(FilePath))
            {
                var message = $"WZ File does not exist at path: '{FilePath}'";
                _log.Error(message);
                throw new FileNotFoundException(message);
            }

            var reader = new WzBinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), _wzIv);

            Header = new WzHeader
            {
                Ident = reader.ReadString(4),
                FSize = reader.ReadUInt64(),
                FStart = reader.ReadUInt32(),
                Copyright = reader.ReadNullTerminatedString()
            };
            reader.ReadBytes((int)(Header.FStart - reader.BaseStream.Position));
            reader.Header = Header;
            _version = reader.ReadInt16();
            if (Version == -1 && !CalculateVersion(reader))
            {
                throw new InvalidDataException(
                    "Error with game version hash : The specified game version is incorrect");
            }

            _versionHash = GetVersionHash(_version, Version);
            reader.Hash = _versionHash;
            var tempDirectory = new WzDirectory(reader, Name, _versionHash, _wzIv, this);
            tempDirectory.ParseDirectory();
            WzDirectory = tempDirectory;
        }

        private bool CalculateVersion(WzBinaryReader reader)
        {
            for (var j = 0; j < short.MaxValue; j++)
            {
                Version = (short)j;
                _versionHash = GetVersionHash(_version, Version);
                if (_versionHash == 0)
                {
                    continue;
                }

                reader.Hash = _versionHash;
                var position = reader.BaseStream.Position;
                WzDirectory testDirectory;
                try
                {
                    testDirectory = new WzDirectory(reader, Name, _versionHash, _wzIv, this);
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
                            var directory = new WzDirectory(reader, Name, _versionHash, _wzIv, this);
                            directory.ParseDirectory();
                            WzDirectory = directory;
                            return true;
                        default:
                            reader.BaseStream.Position = position;
                            return true;
                    }
                }
                catch
                {
                    reader.BaseStream.Position = position;
                }
            }

            return false;
        }

        private static uint GetVersionHash(int encver, int realver)
        {
            var encryptedVersionNumber = encver;
            var versionNumber = realver;
            var versionHash = 0;
            var versionNumberStr = versionNumber.ToString();

            var l = versionNumberStr.Length;
            for (var i = 0; i < l; i++)
            {
                versionHash = 32 * versionHash + versionNumberStr[i] + 1;
            }

            var a = (versionHash >> 24) & 0xFF;
            var b = (versionHash >> 16) & 0xFF;
            var c = (versionHash >> 8) & 0xFF;
            var d = versionHash & 0xFF;
            var decryptedVersionNumber = 0xff ^ a ^ b ^ c ^ d;

            return encryptedVersionNumber == decryptedVersionNumber
                ? Convert.ToUInt32(versionHash)
                : 0;
        }

        private void CreateVersionHash()
        {
            _versionHash = 0;
            foreach (var ch in Version.ToString())
            {
                _versionHash = _versionHash * 32 + (byte)ch + 1;
            }

            uint a = (_versionHash >> 24) & 0xFF,
                b = (_versionHash >> 16) & 0xFF,
                c = (_versionHash >> 8) & 0xFF,
                d = _versionHash & 0xFF;
            _version = (byte)~(a ^ b ^ c ^ d);
        }

        /// <summary>
        /// Saves a wz file to the disk, AKA repacking.
        /// </summary>
        /// <param name="path">Path to the output wz file</param>
        public void SaveToDisk(string path)
        {
            _wzIv = WzTool.GetIvByMapleVersion(MapleVersionType);
            CreateVersionHash();
            WzDirectory.SetHash(_versionHash);
            var tempFile = Path.GetFileNameWithoutExtension(path) + ".TEMP";
            File.Create(tempFile).Close();
            WzDirectory.GenerateDataFile(tempFile);
            WzTool.StringCache.Clear();
            var totalLen = WzDirectory.GetImgOffsets(WzDirectory.GetOffsets(Header.FStart + 2));
            var wzWriter = new WzBinaryWriter(File.Create(path), _wzIv) {Hash = _versionHash};
            Header.FSize = totalLen - Header.FStart;
            for (var i = 0; i < 4; i++)
            {
                wzWriter.Write((byte)Header.Ident[i]);
            }

            wzWriter.Write((long)Header.FSize);
            wzWriter.Write(Header.FStart);
            wzWriter.WriteNullTerminatedString(Header.Copyright);
            var extraHeaderLength = Header.FStart - wzWriter.BaseStream.Position;
            if (extraHeaderLength > 0)
            {
                wzWriter.Write(new byte[(int)extraHeaderLength]);
            }

            wzWriter.Write(_version);
            wzWriter.Header = Header;
            WzDirectory.SaveDirectory(wzWriter);
            wzWriter.StringCache.Clear();
            var fs = File.OpenRead(tempFile);
            WzDirectory.SaveImages(wzWriter, fs);
            fs.Close();
            File.Delete(tempFile);
            wzWriter.StringCache.Clear();
            wzWriter.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            if (path.ToLower() == Name.ToLower())
            {
                return new List<WzObject> {WzDirectory};
            }

            if (path == "*")
            {
                var fullList = new List<WzObject> {WzDirectory};
                fullList.AddRange(WzDirectory.GetObjects());
                return fullList;
            }

            if (!path.Contains("*"))
            {
                return new List<WzObject> {GetObjectFromPath(path)};
            }

            var seperatedNames = path.Split("/".ToCharArray());
            if (seperatedNames.Length == 2 && seperatedNames[1] == "*")
            {
                return WzDirectory.GetObjects().ToList();
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                foreach (var spath in img.GetPaths(Name + "/" + img.Name))
                {
                    if (StrMatch(path, spath))
                    {
                        objList.Add(GetObjectFromPath(spath));
                    }
                }
            }

            foreach (var dir in WzDirectory.WzDirectories)
            {
                foreach (var spath in dir.GetPaths(Name + "/" + dir.Name))
                {
                    if (StrMatch(path, spath))
                    {
                        objList.Add(GetObjectFromPath(spath));
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromRegexPath(string path)
        {
            if (path.ToLower() == Name.ToLower())
            {
                return new List<WzObject> {WzDirectory};
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                foreach (var spath in img.GetPaths(Name + "/" + img.Name))
                {
                    if (Regex.Match(spath, path).Success)
                    {
                        objList.Add(GetObjectFromPath(spath));
                    }
                }
            }

            foreach (var dir in WzDirectory.WzDirectories)
            {
                foreach (var spath in dir.GetPaths(Name + "/" + dir.Name))
                {
                    if (Regex.Match(spath, path).Success)
                    {
                        objList.Add(GetObjectFromPath(spath));
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public WzObject GetObjectFromPath(string path)
        {
            var seperatedPath = path.Split("/".ToCharArray());
            if (seperatedPath[0].ToLower() != WzDirectory.Name.ToLower() && seperatedPath[0].ToLower() !=
                WzDirectory.Name.Substring(0, WzDirectory.Name.Length - 3).ToLower())
            {
                return null;
            }

            if (seperatedPath.Length == 1)
            {
                return WzDirectory;
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
                        switch (((WzImageProperty)curObj).Type)
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
                                switch (seperatedPath[i])
                                {
                                    case "X":
                                        return ((WzVectorProperty)curObj).X;
                                    case "Y":
                                        return ((WzVectorProperty)curObj).Y;
                                    default:
                                        return null;
                                }

                            case WzPropertyType.Null:
                            case WzPropertyType.Short:
                            case WzPropertyType.Int:
                            case WzPropertyType.Long:
                            case WzPropertyType.Float:
                            case WzPropertyType.Double:
                            case WzPropertyType.String:
                            case WzPropertyType.Sound:
                            case WzPropertyType.Uol:
                            case WzPropertyType.Png:
                                return null;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    case WzObjectType.File:
                    case WzObjectType.List:
                        throw new InvalidOperationException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return curObj;
        }

        internal static bool StrMatch(string strWildCard, string strCompare)
        {
            if (strWildCard.Length == 0)
            {
                return strCompare.Length == 0;
            }

            if (strCompare.Length == 0)
            {
                return false;
            }

            if (strWildCard[0] == '*' && strWildCard.Length > 1)
            {
                return strCompare.Where((t, index) => StrMatch(strWildCard.Substring(1), strCompare.Substring(index)))
                    .Any();
            }

            if (strWildCard[0] != '*')
            {
                return strWildCard[0] == strCompare[0] && StrMatch(strWildCard.Substring(1), strCompare.Substring(1));
            }

            return true;
        }

        public override void Remove() => Dispose();
    }
}
