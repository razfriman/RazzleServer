using System.Collections.Generic;
using System.IO;
using System;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib
{
	/// <summary>
	/// A .img contained in a wz directory
	/// </summary>
	public class WzImage : WzSubProperty
	{
		#region Fields
		internal bool mParsed = false;
		internal int mSize, checksum;
		internal uint mOffset = 0;
		internal WzBinaryReader mReader;
		internal int mBlockStart = 0;
		internal long mTempFileStart = 0;
		internal long mTempFileEnd = 0;
		#endregion

		/// <summary>
		/// The parent of the object
		/// </summary>
		public override AWzObject Parent { get { return mParent; } internal set { mParent = value; } }
		/// <summary>
		/// The name of the image
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }
		/// <summary>
		/// Is the object parsed
		/// </summary>
		public bool Parsed { get { return mParsed; } }
		/// <summary>
		/// The size in the wz file of the image
		/// </summary>
		public int BlockSize { get { return mSize; } set { mSize = value; } }
		/// <summary>
		/// The checksum of the image
		/// </summary>
		public int Checksum { get { return checksum; } set { checksum = value; } }
		/// <summary>
		/// The offset of the image
		/// </summary>
		public uint Offset { get { return mOffset; } set { mOffset = value; } }
		public int BlockStart { get { return mBlockStart; } }
		/// <summary>
		/// The properties contained in the image
		/// </summary>
		public override List<AWzImageProperty> WzProperties
		{
			get
			{
				if (mReader != null && !mParsed)
				{
					ParseImage();
				}
                return mProperties;
			}
		}

		/// <summary>
		/// Gets a wz property by it's name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		/// <returns>The wz property with the specified name</returns>
		public override AWzImageProperty this[string pName]
		{
			get
			{
				if (mReader != null && !mParsed) ParseImage();
				foreach (AWzImageProperty iwp in mProperties)
					if (iwp.Name.ToLower() == pName.ToLower())
                        return iwp;
                return null;
			}
		}

		/// <summary>
		/// The WzObjectType of the image
		/// </summary>
		public override WzObjectType ObjectType { get { if (mReader != null && !mParsed) ParseImage(); return WzObjectType.Image; } }

		/// <summary>
		/// Creates a blank WzImage
		/// </summary>
		public WzImage() { }
		/// <summary>
		/// Creates a WzImage with the given name
		/// </summary>
		/// <param name="pName">The name of the image</param>
		public WzImage(string pName)
		{
			this.mName = pName;
		}
		public WzImage(string pName, Stream pDataStream, WzMapleVersion pMapleVersion)
		{
			this.mName = pName;
			this.mReader = new WzBinaryReader(pDataStream, WzTool.GetIvByMapleVersion(pMapleVersion));
		}
		internal WzImage(string pName, WzBinaryReader pReader)
		{
			this.mName = pName;
			this.mReader = pReader;
			this.mBlockStart = (int)pReader.BaseStream.Position;
		}

		public override void Dispose()
		{
			mName = null;
			mReader = null;
			if (mProperties != null)
			{
				foreach (AWzImageProperty prop in mProperties)
					prop.Dispose();
				mProperties.Clear();
				mProperties = null;
			}
		}


		/// <summary>
		/// Parses the image from the wz filetod
		/// </summary>
		/// <param name="wzReader">The BinaryReader that is currently reading the wz file</param>
		public void ParseImage()
		{
			long originalPos = mReader.BaseStream.Position;
			mReader.BaseStream.Position = mOffset;
			byte b = mReader.ReadByte();
			if (b != 0x73 || mReader.ReadString() != "Property" || mReader.ReadUInt16() != 0)
				return;
			mProperties.AddRange(AWzImageProperty.ParsePropertyList(mOffset, mReader, this, this));
			mParsed = true;
		}

		public byte[] DataBlock
		{
			get
			{
				byte[] blockData = null;
				if (mReader != null && mSize > 0)
				{
					blockData = mReader.ReadBytes(mSize);
					mReader.BaseStream.Position = mBlockStart;
				}
				return blockData;
			}
		}

		public void UnparseImage()
		{
			mParsed = false;
			this.mProperties = new List<AWzImageProperty>();
		}

		internal void SaveImage(WzBinaryWriter pWriter)
		{
            
			if (mReader != null && !mParsed) ParseImage();
			long startPos = pWriter.BaseStream.Position;
            WriteValue(pWriter);
			pWriter.StringCache.Clear();
			mSize = (int)(pWriter.BaseStream.Position - startPos);
		}

		public void ExportXml(StreamWriter pWriter, bool pOneFile, int pLevel)
		{
			if (pOneFile)
			{
				pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.OpenNamedTag("WzImage", this.mName, true));
				AWzImageProperty.DumpPropertyList(pWriter, pLevel, WzProperties);
				pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.CloseTag("WzImage"));
			}
			else
			{
				throw new Exception("Under Construction");
			}
		}
	}
}