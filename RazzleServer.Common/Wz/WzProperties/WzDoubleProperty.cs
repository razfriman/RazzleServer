using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
	/// <summary>
	/// A property that has the value of a double
	/// </summary>
	public class WzDoubleProperty : WzImageProperty
	{
		#region Fields
		internal double val;
		#endregion

		#region Inherited Members
        public override void SetValue(object value)
        {
            val = (double)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzDoubleProperty(Name, val);
            return clone;
        }

		public override object WzValue => Value;

		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.Double;

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
            Name = null;
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
            Name = name;
		}
		/// <summary>
		/// Creates a WzDoubleProperty with the specified name and value
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="value">The value of the property</param>
		public WzDoubleProperty(string name, double value)
		{
            Name = name;
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