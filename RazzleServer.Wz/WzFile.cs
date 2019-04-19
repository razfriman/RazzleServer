using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RazzleServer.Wz.Util;
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
        private readonly byte[] _wzIv;

        public WzDirectory WzDirectory { get; private set; } = new WzDirectory();

        public override WzObjectType ObjectType => WzObjectType.File;

        /// <summary>
        /// Returns WzDirectory[name]
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>WzDirectory[name]</returns>
        public WzObject this[string name] => WzDirectory[name];

        [JsonIgnore] public WzHeader Header { get; set; } = WzHeader.GetDefault();

        public short Version { get; set; }

        [JsonIgnore] public string FilePath { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WzMapleVersionType MapleVersionType { get; }

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
        public void ParseWzFile() => ParseMainWzDirectory(_wzIv);

        public void ParseWzFile(byte[] wzIv) => ParseMainWzDirectory(wzIv);

        private void ParseMainWzDirectory(byte[] wzIv)
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


            var mmf = MemoryMappedFile.CreateFromFile(FilePath);
            var reader = new WzBinaryReader(mmf.CreateViewStream(), wzIv);

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
            var tempDirectory = new WzDirectory(reader, Name, _versionHash, wzIv, this);
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

        public override void Remove() => Dispose();
    }
}
