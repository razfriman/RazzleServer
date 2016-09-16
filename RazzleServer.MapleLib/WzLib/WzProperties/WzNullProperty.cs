using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that's value is null
	/// </summary>
	public class WzNullProperty : AWzImageProperty
	{
		#region Fields
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
		public override WzPropertyType PropertyType { get { return WzPropertyType.Null; } }
		/// <summary>
		/// The name of the property
		/// </summary>
		/// 
		public override string Name { get { return mName; } set { mName = value; } }
		/// <summary>
		/// The WzObjectType of the property
		/// </summary>
		public override WzObjectType ObjectType { get { return WzObjectType.Property; } }

        public override object WzValue { get { return null; } set { } }

		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.Write((byte)0);
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.EmptyNamedTag("WzNull", this.Name));
		}
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
			mName = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// Creates a blank WzNullProperty
		/// </summary>
		public WzNullProperty() { }
		/// <summary>
		/// Creates a WzNullProperty with the specified name
		/// </summary>
		/// <param name="pPropName">The name of the property</param>
		public WzNullProperty(string pPropName)
		{
			mName = pPropName;
		}
		#endregion

	}
}