using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
	/// <summary>
	/// A property with a string as a value
	/// </summary>
	public class WzStringProperty : WzImageProperty
	{
		#region Fields
		internal string val;
		#endregion

		#region Inherited Members
        public override void SetValue(object value)
        {
            val = (string)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzStringProperty(Name, val);
            return clone;
        }

		public override object WzValue => Value;

		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.String;

		public override void WriteValue(WzBinaryWriter writer)
		{
			writer.Write((byte)8);
			writer.WriteStringValue(Value, 0, 1);
		}
		public override void ExportXml(StreamWriter writer, int level)
		{
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzString", Name, Value));
		}
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
            Name = null;
			val = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The value of the property
		/// </summary>
		public string Value { get => val;
			set => val = value;
		}
		/// <summary>
		/// Creates a blank WzStringProperty
		/// </summary>
		public WzStringProperty() { }
		/// <summary>
		/// Creates a WzStringProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		public WzStringProperty(string name)
		{
            Name = name;
		}
		/// <summary>
		/// Creates a WzStringProperty with the specified name and value
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="value">The value of the property</param>
		public WzStringProperty(string name, string value)
		{
            Name = name;
			val = value;
		}
        #endregion

        #region Cast Values
        public override float GetFloat() => float.TryParse(val, out var result) ? result : 0;

        public override double GetDouble() => double.TryParse(val, out var result) ? result : 0;

        public override int GetInt() => int.TryParse(val, out var result) ? result : 0;

        public override short GetShort() => short.TryParse(val, out var result) ? result : (short)0;

        public override long GetLong() => long.TryParse(val, out var result) ? result : 0;

        public override string GetString() => val;

        public override string ToString() => val;
        #endregion
	}
}