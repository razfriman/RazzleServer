using ProtoBuf;

namespace RazzleServer.Wz.WzProperties
{
    [ProtoContract]public class WzLongProperty : WzImageProperty
    {
        public override WzImageProperty DeepClone() => new WzLongProperty(Name, Value);

        public override object WzValue => Value;

        public override WzPropertyType Type => WzPropertyType.Long;

        public override void Dispose() => Name = null;

        /// <summary>
        /// The value of the property
        /// </summary>
        [ProtoMember(1)]public long Value { get; set; }

        /// <summary>
        /// Creates a blank WzCompressedIntProperty
        /// </summary>
        public WzLongProperty() { }

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzLongProperty(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzLongProperty(string name, long value)
        {
            Name = name;
            Value = value;
        }

        public override float GetFloat()
        {
            return Value;
        }

        public override double GetDouble()
        {
            return Value;
        }

        public override long GetLong()
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

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
