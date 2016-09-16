using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that is stored in the wz file with a signed byte and possibly followed by an int. If the 
	/// signed byte is equal to -128, the value is is the int that follows, else the value is the byte.
	/// </summary>
	public class WzCompressedIntProperty : AWzImageProperty
	{
		#region Fields
		internal string mName;
		internal int mVal;
		internal AWzObject mParent;
		internal WzImage mImgParent;
		#endregion

		#region Inherited Members
        public override object WzValue { get { return mVal; } set { mVal = (int)value; } }
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
		public override WzPropertyType PropertyType { get { return WzPropertyType.CompressedInt; } }
		/// <summary>
		/// The name of the property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }
		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.Write((byte)3);
			pWriter.WriteCompressedInt(Value);
		}
		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.EmptyNamedValuePair("WzCompressedInt", this.Name, this.Value.ToString()));
		}
		/// <summary>
		/// Dispose the object
		/// </summary>
		public override void Dispose()
		{
			mName = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The value of the property
		/// </summary>
		public int Value { get { return mVal; } set { mVal = value; } }
		/// <summary>
		/// Creates a blank WzCompressedIntProperty
		/// </summary>
		public WzCompressedIntProperty() { }
		/// <summary>
		/// Creates a WzCompressedIntProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzCompressedIntProperty(string pName)
		{
			this.mName = pName;
		}
		/// <summary>
		/// Creates a WzCompressedIntProperty with the specified name and value
		/// </summary>
		/// <param name="pName">The name of the property</param>
		/// <param name="pValue">The value of the property</param>
		public WzCompressedIntProperty(string pName, int pValue)
		{
			this.mName = pName;
			this.mVal = pValue;
		}
        #endregion

        #region Cast Values
        public override float ToFloat(float pDef)
        {
            return (float)mVal;
        }

        public override double ToDouble(double pDef)
        {
            return (double)mVal;
        }

        public override int ToInt(int pDef)
        {
            return mVal;
        }

        public override ushort ToUnsignedShort(ushort pDef)
        {
            return (ushort)mVal;
        }
        #endregion

	}
}