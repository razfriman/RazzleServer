using ProtoBuf;

namespace RazzleServer.Wz.WzProperties
{
    /// <inheritdoc />
    /// <summary>
    /// A property that is stored in the wz file with a signed byte and possibly followed by an int. If the 
    /// signed byte is equal to -128, the value is is the int that follows, else the value is the byte.
    /// </summary>
    [ProtoContract]
    public class WzIntProperty : WzImageProperty
    {
        /// <summary>
        /// The value of the property
        /// </summary>
        [ProtoMember(1)]
        public int Value { get; set; }

        public override WzImageProperty DeepClone() => new WzIntProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.Int;

        public override void Dispose() => Name = null;

        /// <summary>
        /// Creates a blank WzCompressedIntProperty
        /// </summary>
        public WzIntProperty() { }

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzIntProperty(string name) => Name = name;

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzIntProperty(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public override float GetFloat() => Value;

        public override double GetDouble() => Value;

        public override int GetInt() => Value;

        public override short GetShort() => (short)Value;

        public override long GetLong() => Value;

        public override string ToString() => Value.ToString();
    }
}
