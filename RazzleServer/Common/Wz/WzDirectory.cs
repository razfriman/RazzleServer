using System;
using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz
{
	/// <summary>
	/// A directory in the wz file, which may contain sub directories or wz images
	/// </summary>
	public class WzDirectory : WzObject
	{
		#region Fields
		internal List<WzImage> images = new List<WzImage>();
		internal List<WzDirectory> subDirs = new List<WzDirectory>();
		internal WzBinaryReader reader;
		internal uint offset;
		internal string name;
		internal uint hash;
		internal int size, checksum, offsetSize;
		internal byte[] WzIv;
		internal WzObject parent;
        internal WzFile wzFile;
		#endregion

		#region Inherited Members
		/// <summary>  
		/// The parent of the object
		/// </summary>
		public override WzObject Parent { get => parent;
			internal set => parent = value;
		}
		/// <summary>
		/// The name of the directory
		/// </summary>
		public override string Name { get => name;
			set => name = value;
		}
		/// <summary>
		/// The WzObjectType of the directory
		/// </summary>
		public override WzObjectType ObjectType => WzObjectType.Directory;

		public override /*I*/WzFile WzFileParent => wzFile;

		/// <summary>
		/// Disposes the obejct
		/// </summary>
		public override void Dispose()
		{
			name = null;
			reader = null;
            images?.ForEach(x => x.Dispose());
            subDirs?.ForEach(x => x.Dispose());
			images.Clear();
			subDirs.Clear();
			images = null;
			subDirs = null;
		}
		#endregion

        #region Custom Members
        /// <summary>
		/// The size of the directory in the wz file
		/// </summary>
		public int BlockSize { get => size;
	        set => size = value;
        }
		/// <summary>
		/// The directory's chceksum
		/// </summary>
		public int Checksum { get => checksum;
			set => checksum = value;
		}
		/// <summary>
		/// The wz images contained in the directory
		/// </summary>
		public List<WzImage> WzImages => images;

		/// <summary>
		/// The sub directories contained in the directory
		/// </summary>
		public List<WzDirectory> WzDirectories => subDirs;

		/// <summary>
		/// Offset of the folder
		/// </summary>
		public uint Offset { get => offset;
			set => offset = value;
		}
		/// <summary>
		/// Returns a WzImage or a WzDirectory with the given name
		/// </summary>
		/// <param name="name">The name of the img or dir to find</param>
		/// <returns>A WzImage or WzDirectory</returns>
		public new WzObject this[string name]
		{
			get
			{
				foreach (var i in images)
				{
					if (i.Name.ToLower() == name.ToLower())
					{
						return i;
					}
				}

				foreach (var d in subDirs)
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
                if (value != null)
                {
                    value.Name = name;
                    if (value is WzDirectory)
                    {
	                    AddDirectory((WzDirectory)value);
                    }
                    else if (value is WzImage)
                    {
	                    AddImage((WzImage)value);
                    }
                    else
                    {
	                    throw new ArgumentException("Value must be a Directory or Image");
                    }
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
		public WzDirectory(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Creates a WzDirectory
		/// </summary>
		/// <param name="reader">The BinaryReader that is currently reading the wz file</param>
		/// <param name="WzIv"></param>
		/// <param name="wzFile">The parent Wz File</param>
		/// <param name="dirName"></param>
		/// <param name="verHash"></param>
		internal WzDirectory(WzBinaryReader reader, string dirName, uint verHash, byte[] WzIv, WzFile wzFile)
		{
			this.reader = reader;
			name = dirName;
			hash = verHash;
			this.WzIv = WzIv;
            this.wzFile = wzFile;
		}

		/// <summary>
		/// Parses the WzDirectory
		/// </summary>
        internal void ParseDirectory()
        {
            var entryCount = reader.ReadCompressedInt();
            for (var i = 0; i < entryCount; i++)
            {
                var type = reader.ReadByte();
                string fname = null;
                int fsize;
                int dirChecksum;
                uint dirOffset;

                long rememberPos = 0;

                switch (type)
                {
                    case 1:
                        var unknown = reader.ReadInt32();
                        reader.ReadInt16();
                        var offs = reader.ReadOffset();
                        continue;
                    case 2:
                        var stringOffset = reader.ReadInt32();
                        rememberPos = reader.BaseStream.Position;
                        reader.BaseStream.Position = reader.Header.FStart + stringOffset;
                        type = reader.ReadByte();
                        fname = reader.ReadString();
                        break;
                    case 3:
                    case 4:
                        fname = reader.ReadString();
                        rememberPos = reader.BaseStream.Position;
                        break;
                    default:
                        break;
                }

                reader.BaseStream.Position = rememberPos;
                fsize = reader.ReadCompressedInt();
                dirChecksum = reader.ReadCompressedInt();
                dirOffset = reader.ReadOffset();
                if (type == 3)
                {
                    var subDir = new WzDirectory(reader, fname, hash, WzIv, wzFile);
                    subDir.BlockSize = fsize;
                    subDir.Checksum = dirChecksum;
                    subDir.Offset = dirOffset;
                    subDir.Parent = this;
                    subDirs.Add(subDir);
                }
                else
                {
                    var img = new WzImage(fname, reader)
                    {
                        BlockSize = fsize,
                        Checksum = dirChecksum,
                        Offset = dirOffset,
                        Parent = this
                    };
                    images.Add(img);
                }
            }

            foreach (var subdir in subDirs)
            {
                reader.BaseStream.Position = subdir.offset;
                subdir.ParseDirectory();
            }
        }

		internal void SaveImages(BinaryWriter wzWriter, FileStream fs)
		{
			foreach (var img in images)
			{
                if (img.Changed)
                {
                    fs.Position = img.tempFileStart;
                    var buffer = new byte[img.size];
                    fs.Read(buffer, 0, img.size);
                    wzWriter.Write(buffer);
                }
                else
                {
                    img.reader.BaseStream.Position = img.tempFileStart;
                    wzWriter.Write(img.reader.ReadBytes((int)(img.tempFileEnd - img.tempFileStart)));
                }
			}
			foreach (var dir in subDirs)
			{
				dir.SaveImages(wzWriter, fs);
			}
		}
		internal int GenerateDataFile(string fileName)
		{
			size = 0;
			var entryCount = subDirs.Count + images.Count;
			if (entryCount == 0)
			{
				offsetSize = 1;
				return size = 0;
			}
			size = WzTool.GetCompressedIntLength(entryCount);
			offsetSize = WzTool.GetCompressedIntLength(entryCount);

			WzBinaryWriter imgWriter = null;
			MemoryStream memStream = null;
			var fileWrite = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            WzImage img;
			for (var i = 0; i < images.Count; i++)
			{
                img = images[i];
                if (img.Changed)
                {
                    memStream = new MemoryStream();
                    imgWriter = new WzBinaryWriter(memStream, WzIv);
                    img.SaveImage(imgWriter);
                    img.checksum = 0;
                    foreach (var b in memStream.ToArray())
                    {
                        img.checksum += b;
                    }
                    img.tempFileStart = fileWrite.Position;
                    fileWrite.Write(memStream.ToArray(), 0, (int)memStream.Length);
                    img.tempFileEnd = fileWrite.Position;
                    memStream.Dispose();
                }
                else
                {
                    img.tempFileStart = img.offset;
                    img.tempFileEnd = img.offset + img.size;
                }
                img.UnparseImage();

				var nameLen = WzTool.GetWzObjectValueLength(img.name, 4);
				size += nameLen;
				var imgLen = img.size;
				size += WzTool.GetCompressedIntLength(imgLen);
				size += imgLen;
				size += WzTool.GetCompressedIntLength(img.Checksum);
				size += 4;
				offsetSize += nameLen;
				offsetSize += WzTool.GetCompressedIntLength(imgLen);
				offsetSize += WzTool.GetCompressedIntLength(img.Checksum);
				offsetSize += 4;
                if (img.Changed)
                {
	                imgWriter.Close();
                }
			}
			fileWrite.Close();

            WzDirectory dir;
			for (var i = 0; i < subDirs.Count; i++)
			{
                dir = subDirs[i];
				var nameLen = WzTool.GetWzObjectValueLength(dir.name, 3);
				size += nameLen;
				size += subDirs[i].GenerateDataFile(fileName);
				size += WzTool.GetCompressedIntLength(dir.size);
				size += WzTool.GetCompressedIntLength(dir.checksum);
				size += 4;
				offsetSize += nameLen;
				offsetSize += WzTool.GetCompressedIntLength(dir.size);
				offsetSize += WzTool.GetCompressedIntLength(dir.checksum);
				offsetSize += 4;
			}
			return size;
		}
		internal void SaveDirectory(WzBinaryWriter writer)
		{
			offset = (uint)writer.BaseStream.Position;
			var entryCount = subDirs.Count + images.Count;
			if (entryCount == 0)
			{
				BlockSize = 0;
				return;
			}
			writer.WriteCompressedInt(entryCount);
			foreach (var img in images)
			{
				writer.WriteWzObjectValue(img.name, 4);
				writer.WriteCompressedInt(img.BlockSize);
				writer.WriteCompressedInt(img.Checksum);
				writer.WriteOffset(img.Offset);
			}
			foreach (var dir in subDirs)
			{
				writer.WriteWzObjectValue(dir.name, 3);
				writer.WriteCompressedInt(dir.BlockSize);
				writer.WriteCompressedInt(dir.Checksum);
				writer.WriteOffset(dir.Offset);
			}
			foreach (var dir in subDirs)
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
			offset = curOffset;
			curOffset += (uint)offsetSize;
			foreach (var dir in subDirs)
			{
				curOffset = dir.GetOffsets(curOffset);
			}
			return curOffset;
		}
		internal uint GetImgOffsets(uint curOffset)
		{
			foreach (var img in images)
			{
				img.Offset = curOffset;
				curOffset += (uint)img.BlockSize;
			}
			foreach (var dir in subDirs)
			{
				curOffset = dir.GetImgOffsets(curOffset);
			}
			return curOffset;
		}
		internal void ExportXml(StreamWriter writer, bool oneFile, int level, bool isDirectory)
		{
			if (oneFile)
			{
				if (isDirectory)
				{
					writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzDirectory", name, true));
				}
				foreach (var subDir in WzDirectories)
				{
					subDir.ExportXml(writer, oneFile, level + 1, isDirectory);
				}
				foreach (var subImg in WzImages)
				{
					subImg.ExportXml(writer, oneFile, level + 1);
				}
				if (isDirectory)
				{
					writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzDirectory"));
				}
			}
		}
		/// <summary>
		/// Parses the wz images
		/// </summary>
		public void ParseImages()
		{
			foreach (var img in images)
			{
                reader.BaseStream.Position = img.Offset;
                img.ParseImage();
			}
			foreach (var subdir in subDirs)
			{
                reader.BaseStream.Position = subdir.Offset;
                subdir.ParseImages();
			}
		}

		internal void SetHash(uint newHash)
		{
			hash = newHash;
			foreach (var dir in subDirs)
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
			images.Add(img);
            img.Parent = this;
		}
		/// <summary>
		/// Adds a WzDirectory to the list of sub directories
		/// </summary>
		/// <param name="dir">The WzDirectory to add</param>
		public void AddDirectory(WzDirectory dir)
		{
			subDirs.Add(dir);
            dir.wzFile = wzFile;
            dir.Parent = this;
		}
		/// <summary>
		/// Clears the list of images
		/// </summary>
		public void ClearImages()
		{
            foreach (var img in images)
            {
	            img.Parent = null;
            }

			images.Clear();
		}
		/// <summary>
		/// Clears the list of sub directories
		/// </summary>
		public void ClearDirectories()
		{
            foreach (var dir in subDirs)
            {
	            dir.Parent = null;
            }

			subDirs.Clear();
		}
		/// <summary>
		/// Gets an image in the list of images by it's name
		/// </summary>
		/// <param name="name">The name of the image</param>
		/// <returns>The wz image that has the specified name or null if none was found</returns>
		public WzImage GetImageByName(string name)
		{
			foreach (var wzI in images)
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
			foreach (var dir in subDirs)
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
			imgFiles.AddRange(images);
			foreach (var subDir in subDirs)
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
            images.Remove(image);
            image.Parent = null;
		}
		/// <summary>
		/// Removes a sub directory from the list
		/// </summary>
		/// <param name="dir">The sub directory to remove</param>
		public void RemoveDirectory(WzDirectory dir)
		{
            subDirs.Remove(dir);
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
            var result = images.Count;
            foreach (var subdir in WzDirectories)
            {
	            result += subdir.CountImages();
            }

	        return result;
        }
        #endregion

        public override void Remove()
        {
            ((WzDirectory)Parent).RemoveDirectory(this);
        }
    }
}