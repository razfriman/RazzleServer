using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private int _offsetSize;
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
        public new WzObject this[string name]
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
                        throw new InvalidDataException();
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

        internal int GenerateDataFile(string fileName)
        {
            BlockSize = 0;
            var entryCount = WzDirectories.Count + WzImages.Count;
            if (entryCount == 0)
            {
                _offsetSize = 1;
                return BlockSize = 0;
            }

            BlockSize = WzTool.GetCompressedIntLength(entryCount);
            _offsetSize = WzTool.GetCompressedIntLength(entryCount);

            WzBinaryWriter imgWriter = null;
            var fileWrite = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            foreach (var img in WzImages)
            {
                if (img.Changed)
                {
                    var memStream = new MemoryStream();
                    imgWriter = new WzBinaryWriter(memStream, WzIv);
                    img.SaveImage(imgWriter);
                    img.Checksum = 0;
                    img.Checksum += memStream.ToArray().Sum(x => x);
                    img.TempFileStart = fileWrite.Position;
                    fileWrite.Write(memStream.ToArray());
                    img.TempFileEnd = fileWrite.Position;
                    memStream.Dispose();
                }
                else
                {
                    img.TempFileStart = img.Offset;
                    img.TempFileEnd = img.Offset + img.BlockSize;
                }

                img.UnparseImage();

                var nameLen = WzTool.GetWzObjectValueLength(img.Name, 4);
                BlockSize += nameLen;
                var imgLen = img.BlockSize;
                BlockSize += WzTool.GetCompressedIntLength(imgLen);
                BlockSize += imgLen;
                BlockSize += WzTool.GetCompressedIntLength(img.Checksum);
                BlockSize += 4;
                _offsetSize += nameLen;
                _offsetSize += WzTool.GetCompressedIntLength(imgLen);
                _offsetSize += WzTool.GetCompressedIntLength(img.Checksum);
                _offsetSize += 4;
                if (img.Changed)
                {
                    imgWriter?.Close();
                }
            }

            fileWrite.Close();

            foreach (var dir in WzDirectories)
            {
                var nameLen = WzTool.GetWzObjectValueLength(dir.Name, 3);
                BlockSize += nameLen;
                BlockSize += dir.GenerateDataFile(fileName);
                BlockSize += WzTool.GetCompressedIntLength(dir.BlockSize);
                BlockSize += WzTool.GetCompressedIntLength(dir.Checksum);
                BlockSize += 4;
                _offsetSize += nameLen;
                _offsetSize += WzTool.GetCompressedIntLength(dir.BlockSize);
                _offsetSize += WzTool.GetCompressedIntLength(dir.Checksum);
                _offsetSize += 4;
            }

            return BlockSize;
        }

        internal void SaveDirectory(WzBinaryWriter writer)
        {
            Offset = (uint)writer.BaseStream.Position;
            var entryCount = WzDirectories.Count + WzImages.Count;
            if (entryCount == 0)
            {
                BlockSize = 0;
                return;
            }

            writer.WriteCompressedInt(entryCount);
            foreach (var img in WzImages)
            {
                writer.WriteWzObjectValue(img.Name, 4);
                writer.WriteCompressedInt(img.BlockSize);
                writer.WriteCompressedInt(img.Checksum);
                writer.WriteOffset(img.Offset);
            }

            foreach (var dir in WzDirectories)
            {
                writer.WriteWzObjectValue(dir.Name, 3);
                writer.WriteCompressedInt(dir.BlockSize);
                writer.WriteCompressedInt(dir.Checksum);
                writer.WriteOffset(dir.Offset);
            }

            foreach (var dir in WzDirectories)
            {
                if (dir.BlockSize > 0)
                {
                    dir.SaveDirectory(writer);
                }
                else
                {
                    writer.Write((byte)0);
                }
            }
        }

        internal uint GetOffsets(uint curOffset)
        {
            Offset = curOffset;
            curOffset += (uint)_offsetSize;
            foreach (var dir in WzDirectories)
            {
                curOffset = dir.GetOffsets(curOffset);
            }

            return curOffset;
        }

        internal uint GetImgOffsets(uint curOffset)
        {
            foreach (var img in WzImages)
            {
                img.Offset = curOffset;
                curOffset += (uint)img.BlockSize;
            }

            foreach (var dir in WzDirectories)
            {
                curOffset = dir.GetImgOffsets(curOffset);
            }

            return curOffset;
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

        public int CountImages()
        {
            var result = WzImages.Count;
            foreach (var subdir in WzDirectories)
            {
                result += subdir.CountImages();
            }

            return result;
        }

        public override void Remove() => ((WzDirectory)Parent)?.RemoveDirectory(this);

        public override IEnumerable<WzObject> GetObjects()
        {
            var objList = new List<WzObject>();
            foreach (var img in WzImages)
            {
                objList.Add(img);
                objList.AddRange(img.GetObjects());
            }

            foreach (var subdir in WzDirectories)
            {
                objList.Add(subdir);
                objList.AddRange(subdir.GetObjects());
            }

            return objList;
        }

        internal IEnumerable<string> GetPaths(string curPath)
        {
            var objList = new List<string>();
            foreach (var img in WzImages)
            {
                objList.Add(curPath + "/" + img.Name);
                objList.AddRange(img.GetPaths(curPath + "/" + img.Name));
            }

            foreach (var subdir in WzDirectories)
            {
                objList.Add(curPath + "/" + subdir.Name);
                objList.AddRange(subdir.GetPaths(curPath + "/" + subdir.Name));
            }

            return objList;
        }
    }
}
