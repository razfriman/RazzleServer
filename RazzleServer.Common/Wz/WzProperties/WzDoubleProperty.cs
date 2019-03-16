using System.Globalization;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
	/// <summary>
	/// A property that has the value of a double
	/// </summary>
	public class WzDoubleProperty : WzImageProperty
	{
		#region Inherited Members
        public override void SetValue(object value)
        {
            Value = (double)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzDoubleProperty(Name, Value);
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
		
		public override void Dispose()
		{
            Name = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The value of this property
		/// </summary>
        public double Value { get; set; }

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
			Value = value;
		}
		#endregion

        #region Cast Values
        public override float GetFloat()
        {
            return (float)Value;
        }

        public override double GetDouble()
        {
            return Value;
        }

        public override int GetInt()
        {
            return (int)Value;
        }

        public override short GetShort()
        {
            return (short)Value;
        }

        public override long GetLong()
        {
            return (long)Value;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.CurrentCulture);
        }
        #endregion
	}
}