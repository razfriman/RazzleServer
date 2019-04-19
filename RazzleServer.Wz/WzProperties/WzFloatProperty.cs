using System.Globalization;

namespace RazzleServer.Wz.WzProperties
{
    /// <inheritdoc />
    /// <summary>
    /// A property that is stored in the wz file with a byte and possibly followed by a float. If the 
    /// byte is 0, the value is 0, else the value is the float that follows.
    /// </summary>
    public class WzFloatProperty : WzImageProperty
    {
        /// <summary>
        /// Creates a blank WzFloatProperty
        /// </summary>
        public WzFloatProperty()
        {
        }

        /// <summary>
        /// Creates a WzByteFloatProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzFloatProperty(string name) => Name = name;

        /// <summary>
        /// Creates a WzByteFloatProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzFloatProperty(string name, float value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// The value of the property
        /// </summary>
        public float Value { get; private set; }

        public override object WzValue => Value;

        public override WzImageProperty DeepClone() => new WzFloatProperty(Name, Value);

        public override WzPropertyType Type => WzPropertyType.Float;

        public override float GetFloat() => Value;

        public override double GetDouble() => Value;

        public override int GetInt() => (int)Value;

        public override short GetShort() => (short)Value;

        public override long GetLong() => (long)Value;

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        public override void Dispose() => Name = null;
    }
}
