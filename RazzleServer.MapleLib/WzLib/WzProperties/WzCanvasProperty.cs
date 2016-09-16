using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;
using RazzleServer.DB.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that can contain sub properties and has one png image
	/// </summary>
	public class WzCanvasProperty : APropertyContainer
	{
		#region Fields
		internal List<AWzImageProperty> mProperties = new List<AWzImageProperty>();
		internal WzPngProperty mImageProp;
		internal string mName;
		internal AWzObject mParent;
		internal WzImage mImgParent;
		#endregion

		#region Inherited Members
        public override object WzValue
        {
            get { return PngProperty; }
            set
            {
                mImageProp.WzValue = value;
            }
        }
		/// <summary>
		/// The parent of the object
		/// </summary>
		public override AWzObject Parent { get { return mParent; } internal set { mParent = value; } }
		/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return mImgParent; } internal set { mImgParent = value; } }
		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType { get { return WzPropertyType.Canvas; } }
		/// <summary>
		/// The properties contained in this property
		/// </summary>
        public override List<AWzImageProperty> WzProperties { get { return mProperties; } }
		/// <summary>
		/// The name of the property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }
		/// <summary>
		/// Gets a wz property by it's name
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <returns>The wz property with the specified name</returns>
		
		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.WriteStringValue("Canvas", 0x73, 0x1B);
			pWriter.Write((byte)0);
			if (mProperties.Count > 0)
			{
				pWriter.Write((byte)1);
				AWzImageProperty.WritePropertyList(pWriter, mProperties);
			}
			else
			{
				pWriter.Write((byte)0);
			}
			pWriter.WriteCompressedInt(PngProperty.Width);
			pWriter.WriteCompressedInt(PngProperty.Height);
			pWriter.WriteCompressedInt(PngProperty.mFormat);
			pWriter.Write((byte)PngProperty.mFormat2);
			pWriter.Write(0);
            byte[] bytes = PngProperty.GetCompressedBytes(false);
			pWriter.Write(bytes.Length + 1);
			pWriter.Write((byte)0);
			pWriter.Write(bytes);
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.OpenNamedTag("WzCanvas", this.Name, false, false) +
			XmlUtil.Attrib("width", PngProperty.Width.ToString()) +
			XmlUtil.Attrib("height", PngProperty.Height.ToString(), true, false));
			AWzImageProperty.DumpPropertyList(pWriter, pLevel, this.WzProperties);
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.CloseTag("WzCanvas"));
		}
		/// <summary>
		/// Dispose the object
		/// </summary>
		public override void Dispose()
		{
			mName = null;
			mImageProp.Dispose();
			mImageProp = null;
			foreach (AWzImageProperty prop in mProperties)
			{
				prop.Dispose();
			}
			mProperties.Clear();
			mProperties = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The png image for this canvas property
		/// </summary>
		public WzPngProperty PngProperty { get { return mImageProp; } set { mImageProp = value; } }
		/// <summary>
		/// Creates a blank WzCanvasProperty
		/// </summary>
		public WzCanvasProperty() { }
		/// <summary>
		/// Creates a WzCanvasProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzCanvasProperty(string pName)
		{
			this.mName = pName;
		}
		#endregion

        #region Cast Values
        internal override WzPngProperty ToPngProperty(WzPngProperty pDef)
        {
            return mImageProp;
        }
        #endregion

	}
}