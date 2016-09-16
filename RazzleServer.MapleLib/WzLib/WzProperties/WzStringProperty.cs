using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property with a string as a value
	/// </summary>
	public class WzStringProperty : AWzImageProperty
	{
		#region Fields
		internal string mName, mVal;
		internal AWzObject mParent;
		internal WzImage mImgParent;
		#endregion

		#region Inherited Members
        public override object WzValue { get { return mVal; } set { mVal = (string)value; } }
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
		public override WzPropertyType PropertyType { get { return WzPropertyType.String; } }
		/// <summary>
		/// The name of the property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }
		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.Write((byte)8);
			pWriter.WriteStringValue(Value, 0, 1);
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.EmptyNamedValuePair("WzString", this.Name, this.Value));
		}
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
			mName = null;
			mVal = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The value of the property
		/// </summary>
		public string Value { get { return mVal; } set { mVal = value; } }
		/// <summary>
		/// Creates a blank WzStringProperty
		/// </summary>
		public WzStringProperty() { }
		/// <summary>
		/// Creates a WzStringProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzStringProperty(string pName)
		{
			this.mName = pName;
		}
		/// <summary>
		/// Creates a WzStringProperty with the specified name and value
		/// </summary>
		/// <param name="pName">The name of the property</param>
		/// <param name="pValue">The value of the property</param>
		public WzStringProperty(string pName, string pValue)
		{
			this.mName = pName;
			this.mVal = pValue;
		}
        #endregion

        #region Cast Values
        public override float ToFloat(float pDef)
        {
            return float.Parse(mVal);
        }

        public override double ToDouble(double pDef)
        {
            return double.Parse(mVal);
        }

        public override int ToInt(int pDef)
        {
            return int.Parse(mVal);
        }

        public override ushort ToUnsignedShort(ushort pDef)
        {
            return ushort.Parse(mVal);
        }

        public override string ToString()
        {
            return mVal;
        }
        #endregion

	}
}