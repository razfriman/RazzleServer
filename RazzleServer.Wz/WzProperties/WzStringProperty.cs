using RazzleServer.Wz.Util;

namespace RazzleServer.Wz.WzProperties
{
    /// <inheritdoc />
    /// <summary>
    /// A property with a string as a value
    /// </summary>
    public class WzStringProperty : WzImageProperty
    {
        /// <summary>
        /// The value of the property
        /// </summary>
        public string Value { get; set; }

        public override void SetValue(object value) => Value = (string)value;

        public override WzImageProperty DeepClone() => new WzStringProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.String;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte)8);
            writer.WriteStringValue(Value, 0, 1);
        }

        public override void Dispose()
        {
            Name = null;
            Value = null;
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
            Value = value;
        }

        public override float GetFloat() => float.TryParse(Value, out var result) ? result : 0;

        public override double GetDouble() => double.TryParse(Value, out var result) ? result : 0;

        public override int GetInt() => int.TryParse(Value, out var result) ? result : 0;

        public override short GetShort() => short.TryParse(Value, out var result) ? result : (short)0;

        public override long GetLong() => long.TryParse(Value, out var result) ? result : 0;

        public override string GetString() => Value;

        public override string ToString() => Value;
    }
}
