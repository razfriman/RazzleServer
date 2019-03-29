using RazzleServer.Wz.Util;

namespace RazzleServer.Wz.WzProperties
{
    /// <inheritdoc />
    /// <summary>
    /// A wz property which has a value which is a ushort
    /// </summary>
    public class WzShortProperty : WzImageProperty
    {
        /// <summary>
        /// The value of the property
        /// </summary>
        public short Value { get; set; }
        
        public override void SetValue(object value) => Value = (short)value;

        public override WzImageProperty DeepClone() => new WzShortProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.Short;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte)2);
            writer.Write(Value);
        }

        public override void Dispose() => Name = null;

        /// <summary>
        /// Creates a blank WzUnsignedShortProperty
        /// </summary>
        public WzShortProperty() { }
        
        /// <summary>
        /// Creates a WzUnsignedShortProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzShortProperty(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// Creates a WzUnsignedShortProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzShortProperty(string name, short value)
        {
            Name = name;
            Value = value;
        }

        public override float GetFloat() => Value;

        public override double GetDouble() => Value;

        public override int GetInt() => Value;

        public override short GetShort() => Value;

        public override long GetLong() => Value;

        public override string ToString() => Value.ToString();
    }
}
