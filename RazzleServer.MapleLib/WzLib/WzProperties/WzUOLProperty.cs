using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;
using RazzleServer.DB.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
	/// <summary>
	/// A property that's value is a string
	/// </summary>
	public class WzUOLProperty : AWzImageProperty, IExtended
	{
		#region Fields
		internal string mName, mVal;
		internal AWzObject mParent;
		internal WzImage mImgParent;
		internal AWzImageProperty mLinkVal;
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
		/// The name of the property
		/// </summary>
		public override string Name { get { return mName; } set { mName = value; } }

        public override List<AWzImageProperty> WzProperties { get { return LinkValue is AWzImageProperty ? LinkValue.WzProperties : null; } }

        public override AWzImageProperty this[string pName] { get { return LinkValue[pName]; } }

		public override AWzImageProperty GetFromPath(string pPath)
		{
			return LinkValue.GetFromPath(pPath);
		}
		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType { get { return WzPropertyType.UOL; } }

		public override void WriteValue(WzBinaryWriter pWriter)
		{
			pWriter.WriteStringValue("UOL", 0x73, 0x1B);
			pWriter.Write((byte)0);
			pWriter.WriteStringValue(Value, 0, 1);
		}

		public override void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.EmptyNamedValuePair("WzUOL", this.Name, this.Value));
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

        public AWzImageProperty LinkValue
        {
            get
            {
                if (mLinkVal == null)
                {
                    string[] paths = mVal.Split('/');
                    mLinkVal = (AWzImageProperty) this.Parent;
                    foreach (string path in paths)
                    {
                        if (path == "..")
                        {
                            mLinkVal = (AWzImageProperty)mLinkVal.Parent;
                        }
                        else
                        {
                            mLinkVal = mLinkVal[path];
                        }
                    }
                }
                return mLinkVal;
            }
        }


		/// <summary>
		/// Creates a blank WzUOLProperty
		/// </summary>
		public WzUOLProperty() { }

		/// <summary>
		/// Creates a WzUOLProperty with the specified name
		/// </summary>
		/// <param name="pName">The name of the property</param>
		public WzUOLProperty(string pName)
		{
			this.mName = pName;
		}

		/// <summary>
		/// Creates a WzUOLProperty with the specified name and value
		/// </summary>
		/// <param name="pName">The name of the property</param>
		/// <param name="pValue">The value of the property</param>
		public WzUOLProperty(string pName, string pValue)
		{
			this.mName = pName;
			this.mVal = pValue;
		}
        #endregion

        #region Cast Values
        public override Bitmap ToBitmap(Bitmap pDef)
        {
            return LinkValue.ToBitmap(pDef);
        }


        public override byte[] ToBytes(byte[] pDef)
        {
            return LinkValue.ToBytes(pDef);
        }

        public override double ToDouble(double pDef)
        {
            return LinkValue.ToDouble(pDef);
        }

        public override float ToFloat(float pDef)
        {
            return LinkValue.ToFloat(pDef);
        }

        public override int ToInt(int pDef)
        {
            return LinkValue.ToInt(pDef);
        }

        public override WzPngProperty ToPngProperty(WzPngProperty pDef)
        {
            return LinkValue.ToPngProperty(pDef);
        }

        public override System.Drawing.Point ToPoint(int pXDef = 0, int pYDef = 0)
        {
            return LinkValue.ToPoint(pXDef, pYDef);
        }

        public override string ToString()
        {
            return LinkValue.ToString();
        }

        public override ushort ToUnsignedShort(ushort pDef)
        {
            return LinkValue.ToUnsignedShort(pDef);
        }
        #endregion

    }
}