using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that contains a set of properties
	/// </summary>
	public class WzSubProperty : APropertyContainer, IExtended
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
		public override WzPropertyType PropertyType { get { return WzPropertyType.SubProperty; } }
		/// <summary>
		/// The wz properties contained in the property
		/// </summary>
        public override List<AWzImageProperty> WzProperties { get { return mProperties; } }
		/// <summary>
		/// The name of the property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }

        public override object WzValue { get { return null; } set { } }

		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.WriteStringValue("Property", 0x73, 0x1B);
			AWzImageProperty.WritePropertyList(pWriter, mProperties);
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.OpenNamedTag("WzSub", this.Name, true));
			AWzImageProperty.DumpPropertyList(pWriter, pLevel, WzProperties);
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.CloseTag("WzSub")); 
		}
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
			mName = null;
			foreach (AWzImageProperty prop in mProperties)
				prop.Dispose();
			mProperties.Clear();
			mProperties = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// Creates a blank WzSubProperty
		/// </summary>
		public WzSubProperty() { }
		/// <summary>
		/// Creates a WzSubProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzSubProperty(string pName)
		{
			this.mName = pName;
		}
		#endregion
	}
}