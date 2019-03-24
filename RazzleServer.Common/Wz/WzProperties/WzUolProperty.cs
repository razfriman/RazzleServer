using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that's value is a string
    /// </summary>
    public class WzUolProperty : WzExtended
    {
        public override void SetValue(object value) => Value = (string)value;

        public override WzImageProperty DeepClone() => new WzUolProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.Uol;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("UOL", 0x73, 0x1B);
            writer.Write((byte)0);
            writer.WriteStringValue(Value, 0, 1);
        }

        public override void Dispose()
        {
            Name = null;
            Value = null;
        }

        /// <summary>
        /// The value of the property
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a blank WzUOLProperty
        /// </summary>
        public WzUolProperty()
        {
        }

        /// <summary>
        /// Creates a WzUOLProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzUolProperty(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a WzUOLProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzUolProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string GetString() => Value;
        
        public override string ToString() => Value;
    }
}
