using System;
using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that contains several WzExtendedPropertys
	/// </summary>
	public class WzConvexProperty : APropertyContainer
	{
		#region Fields
        internal List<AWzImageProperty> mProperties = new List<AWzImageProperty>();
		internal string mName;
		internal AWzObject mParent;
		internal WzImage mImgParent;
		#endregion

		#region Inherited Members
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
		public override WzPropertyType PropertyType { get { return WzPropertyType.Convex; } }
		/// <summary>
		/// The properties contained in the property
		/// </summary>
        public override List<AWzImageProperty> WzProperties { get { return mProperties; } }
		/// <summary>
		/// The name of this property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }

        public override object WzValue { get { return null; } set { } }
		
		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.WriteStringValue("Shape2D#Convex2D", 0x73, 0x1B);
            pWriter.WriteCompressedInt(mProperties.Count);
            foreach (AWzImageProperty prop in mProperties)
            {
                prop.WriteValue(pWriter);
            }
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.OpenNamedTag("WzConvex", this.Name, true));
			AWzImageProperty.DumpPropertyList(pWriter, pLevel, WzProperties);
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.CloseTag("WzConvex"));
		}
		public override void Dispose()
		{
			mName = null;
            foreach (AWzImageProperty prop in mProperties)
                prop.Dispose();
			mProperties.Clear();
			mProperties = null;
		}

        public override void AddProperty(AWzImageProperty pProp)
        {
            if (pProp is IExtended)
            {
                base.AddProperty(pProp);
            }
            else
            {
                throw new Exception("Convex can only hold extended properties");
            }
        }
		#endregion

		#region Custom Members
		/// <summary>
		/// Creates a blank WzConvexProperty
		/// </summary>
		public WzConvexProperty() { }
		/// <summary>
		/// Creates a WzConvexProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzConvexProperty(string pName)
		{
			this.mName = pName;
		}

		#endregion
	}
}