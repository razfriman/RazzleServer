using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RazzleServer.Wz.Util;

namespace RazzleServer.Wz
{
    /// <inheritdoc />
    /// <summary>
    /// A directory in the wz file, which may contain sub directories or wz images
    /// </summary>
    public class WzDirectory : WzObject
    {
        private WzBinaryReader _reader;
        private uint _hash;
        private WzFile _wzFile;

        public override WzObjectType ObjectType => WzObjectType.Directory;

        public override WzFile WzFileParent => _wzFile;

        public override void Dispose()
        {
            Name = null;
            _reader.Dispose();
            _reader = null;
            WzImages?.ForEach(x => x.Dispose());
            WzDirectories?.ForEach(x => x.Dispose());
            WzImages?.Clear();
            WzDirectories?.Clear();
            WzImages = null;
            WzDirectories = null;
        }

        /// <summary>
        /// The size of the directory in the wz file
        /// </summary>
        [JsonIgnore]
        public int BlockSize { get; set; }

        /// <summary>
        /// The directory's checksum
        /// </summary>
        [JsonIgnore]
        public int Checksum { get; set; }

        /// <summary>
        /// The wz images contained in the directory
        /// </summary>
        public List<WzImage> WzImages { get; private set; } = new List<WzImage>();

        /// <summary>
        /// The sub directories contained in the directory
        /// </summary>
        public List<WzDirectory> WzDirectories { get; private set; } = new List<WzDirectory>();

        [JsonIgnore] public byte[] WzIv { get; internal set; }

        /// <summary>
        /// Offset of the folder
        /// </summary>
        [JsonIgnore]
        public uint Offset { get; set; }

        /// <summary>
        /// Returns a WzImage or a WzDirectory with the given name
        /// </summary>
        /// <param name="name">The name of the img or dir to find</param>
        /// <returns>A WzImage or WzDirectory</returns>
        public WzObject this[string name]
        {
            get
            {
                foreach (var i in WzImages)
                {
                    if (i.Name.ToLower() == name.ToLower())
                    {
                        return i;
                    }
                }

                foreach (var d in WzDirectories)
                {
                    if (d.Name.ToLower() == name.ToLower())
                    {
                        return d;
                    }
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                value.Name = name;
                switch (value)
                {
                    case WzDirectory directory:
                        AddDirectory(directory);
                        break;
                    case WzImage image:
                        AddImage(image);
                        break;
                    default:
                        throw new ArgumentException("Value must be a Directory or Image");
                }
            }
        }


        /// <summary>
        /// Creates a blank WzDirectory
        /// </summary>
        public WzDirectory() { }

        /// <summary>
        /// Creates a WzDirectory with the given name
        /// </summary>
        /// <param name="name">The name of the directory</param>
        public WzDirectory(string name) => Name = name;

        /// <summary>
        /// Creates a WzDirectory
        /// </summary>
        /// <param name="reader">The BinaryReader that is currently reading the wz file</param>
        /// <param name="wzIv"></param>
        /// <param name="wzFile">The parent Wz File</param>
        /// <param name="dirName"></param>
        /// <param name="verHash"></param>
        internal WzDirectory(WzBinaryReader reader, string dirName, uint verHash, byte[] wzIv, WzFile wzFile)
        {
            _reader = reader;
            Name = dirName;
            _hash = verHash;
            WzIv = wzIv;
            _wzFile = wzFile;
        }

        /// <summary>
        /// Parses the WzDirectory
        /// </summary>
        internal void ParseDirectory()
        {
            var entryCount = _reader.ReadCompressedInt();
            for (var i = 0; i < entryCount; i++)
            {
                var type = _reader.ReadByte();
                string fname;
                long rememberPos;

                switch (type)
                {
                    case 1:
                        _reader.ReadInt32();
                        _reader.ReadInt16();
                        _reader.ReadOffset();
                        continue;
                    case 2:
                        var stringOffset = _reader.ReadInt32();
                        rememberPos = _reader.BaseStream.Position;
                        _reader.BaseStream.Position = _reader.Header.FStart + stringOffset;
                        type = _reader.ReadByte();
                        fname = _reader.ReadString();
                        break;
                    case 3:
                    case 4:
                        fname = _reader.ReadString();
                        rememberPos = _reader.BaseStream.Position;
                        break;
                    default:
                        throw new InvalidDataException($"Invalid directory type Type={type}");
                }

                _reader.BaseStream.Position = rememberPos;
                var fsize = _reader.ReadCompressedInt();
                var dirChecksum = _reader.ReadCompressedInt();
                var dirOffset = _reader.ReadOffset();
                if (type == 3)
                {
                    var subDir = new WzDirectory(_reader, fname, _hash, WzIv, _wzFile)
                    {
                        BlockSize = fsize, Checksum = dirChecksum, Offset = dirOffset, Parent = this
                    };
                    WzDirectories.Add(subDir);
                }
                else
                {
                    var img = new WzImage(fname, _reader)
                    {
                        BlockSize = fsize, Checksum = dirChecksum, Offset = dirOffset, Parent = this
                    };
                    WzImages.Add(img);
                }
            }

            foreach (var subdir in WzDirectories)
            {
                _reader.BaseStream.Position = subdir.Offset;
                subdir.ParseDirectory();
            }
        }

        internal void SaveImages(BinaryWriter wzWriter, FileStream fs)
        {
            foreach (var img in WzImages)
            {
                if (img.Changed)
                {
                    fs.Position = img.TempFileStart;
                    var buffer = new byte[img.BlockSize];
                    fs.Read(buffer, 0, img.BlockSize);
                    wzWriter.Write(buffer);
                }
                else
                {
                    img.Reader.BaseStream.Position = img.TempFileStart;
                    wzWriter.Write(img.Reader.ReadBytes((int)(img.TempFileEnd - img.TempFileStart)));
                }
            }

            foreach (var dir in WzDirectories)
            {
                dir.SaveImages(wzWriter, fs);
            }
        }

        /// <summary>
        /// Parses the wz images
        /// </summary>
        public void ParseImages()
        {
            foreach (var img in WzImages)
            {
                _reader.BaseStream.Position = img.Offset;
                img.ParseImage();
            }

            foreach (var subdir in WzDirectories)
            {
                _reader.BaseStream.Position = subdir.Offset;
                subdir.ParseImages();
            }
        }

        internal void SetHash(uint newHash)
        {
            _hash = newHash;
            foreach (var dir in WzDirectories)
            {
                dir.SetHash(newHash);
            }
        }

        /// <summary>
        /// Adds a WzImage to the list of wz images
        /// </summary>
        /// <param name="img">The WzImage to add</param>
        public void AddImage(WzImage img)
        {
            WzImages.Add(img);
            img.Parent = this;
        }

        /// <summary>
        /// Adds a WzDirectory to the list of sub directories
        /// </summary>
        /// <param name="dir">The WzDirectory to add</param>
        public void AddDirectory(WzDirectory dir)
        {
            WzDirectories.Add(dir);
            dir._wzFile = WzFileParent;
            dir.Parent = this;
        }

        /// <summary>
        /// Clears the list of images
        /// </summary>
        public void ClearImages()
        {
            foreach (var img in WzImages)
            {
                img.Parent = null;
            }

            WzImages.Clear();
        }

        /// <summary>
        /// Clears the list of sub directories
        /// </summary>
        public void ClearDirectories()
        {
            foreach (var dir in WzDirectories)
            {
                dir.Parent = null;
            }

            WzDirectories.Clear();
        }

        /// <summary>
        /// Gets an image in the list of images by it's name
        /// </summary>
        /// <param name="name">The name of the image</param>
        /// <returns>The wz image that has the specified name or null if none was found</returns>
        public WzImage GetImageByName(string name)
        {
            foreach (var wzI in WzImages)
            {
                if (wzI.Name.ToLower() == name.ToLower())
                {
                    return wzI;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a sub directory in the list of directories by it's name
        /// </summary>
        /// <param name="name">The name of the directory</param>
        /// <returns>The wz directory that has the specified name or null if none was found</returns>
        public WzDirectory GetDirectoryByName(string name)
        {
            foreach (var dir in WzDirectories)
            {
                if (dir.Name.ToLower() == name.ToLower())
                {
                    return dir;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all child images of a WzDirectory
        /// </summary>
        /// <returns></returns>
        public List<WzImage> GetChildImages()
        {
            var imgFiles = new List<WzImage>();
            imgFiles.AddRange(WzImages);
            foreach (var subDir in WzDirectories)
            {
                imgFiles.AddRange(subDir.GetChildImages());
            }

            return imgFiles;
        }

        /// <summary>
        /// Removes an image from the list
        /// </summary>
        /// <param name="image">The image to remove</param>
        public void RemoveImage(WzImage image)
        {
            WzImages.Remove(image);
            image.Parent = null;
        }

        /// <summary>
        /// Removes a sub directory from the list
        /// </summary>
        /// <param name="dir">The sub directory to remove</param>
        public void RemoveDirectory(WzDirectory dir)
        {
            WzDirectories.Remove(dir);
            dir.Parent = null;
        }

        public WzDirectory DeepClone()
        {
            var result = (WzDirectory)MemberwiseClone();
            result.WzDirectories.Clear();
            result.WzImages.Clear();
            foreach (var dir in WzDirectories)
            {
                result.WzDirectories.Add(dir.DeepClone());
            }

            foreach (var img in WzImages)
            {
                result.WzImages.Add(img.DeepClone());
            }

            return result;
        }

        public override void Remove() => ((WzDirectory)Parent)?.RemoveDirectory(this);
    }
}
