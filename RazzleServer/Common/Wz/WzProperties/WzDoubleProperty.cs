using System.IO;
using RazzleServer.Common.WzLib.Util;

namespace RazzleServer.Common.WzLib.WzProperties
{
	/// <summary>
	/// A property that has the value of a double
	/// </summary>
	public class WzDoubleProperty : WzImageProperty
	{
		#region Fields
		internal string name;
		internal double val;
		internal WzObject parent;
		//internal WzImage imgParent;
		#endregion

		#region Inherited Members
        public override void SetValue(object value)
        {
            val = (double)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzDoubleProperty(name, val);
            return clone;
        }

		public override object WzValue => Value;

		/// <summary>
		/// The parent of the object
		/// </summary>
		public override WzObject Parent { get => parent;
			internal set => parent = value;
		}
		/*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.Double;

		/// <summary>
		/// The name of this property
		/// </summary>
		public override string Name { get => name;
			set => name = value;
		}
		public override void WriteValue(WzBinaryWriter writer)
		{
			writer.Write((byte)5);
			writer.Write(Value);
		}
		public override void ExportXml(StreamWriter writer, int level)
		{
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzDouble", Name, Value.ToString()));
		}
		public override void Dispose()
		{
			name = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The value of this property
		/// </summary>
		public double Value { get => val;
			set => val = value;
		}
		/// <summary>
		/// Creates a blank WzDoubleProperty
		/// </summary>
		public WzDoubleProperty() { }
		/// <summary>
		/// Creates a WzDoubleProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		public WzDoubleProperty(string name)
		{
			this.name = name;
		}
		/// <summary>
		/// Creates a WzDoubleProperty with the specified name and value
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="value">The value of the property</param>
		public WzDoubleProperty(string name, double value)
		{
			this.name = name;
			val = value;
		}
		#endregion

        #region Cast Values
        public override float GetFloat()
        {
            return (float)val;
        }

        public override double GetDouble()
        {
            return val;
        }

        public override int GetInt()
        {
            return (int)val;
        }

        public override short GetShort()
        {
            return (short)val;
        }

        public override long GetLong()
        {
            return (long)val;
        }

        public override string ToString()
        {
            return val.ToString();
        }
        #endregion
	}
}