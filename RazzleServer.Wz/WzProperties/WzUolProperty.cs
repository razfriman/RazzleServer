using ProtoBuf;

namespace RazzleServer.Wz.WzProperties
{
    /// <summary>
    /// A property that's value is a string
    /// </summary>
    [ProtoContract]public class WzUolProperty : WzImageProperty
    {
        public override WzImageProperty DeepClone() => new WzUolProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.Uol;

        public override void Dispose()
        {
            Name = null;
            Value = null;
        }

        /// <summary>
        /// The value of the property
        /// </summary>
        [ProtoMember(1)] public string Value { get; set; }

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
