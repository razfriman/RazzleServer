using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A wz property which has a value which is a ushort
    /// </summary>
    public class WzShortProperty : WzImageProperty
    {
        #region Fields
        internal short val;
        #endregion

        #region Inherited Members
        public override void SetValue(object value)
        {
            val = (short)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzShortProperty(Name, val);
            return clone;
        }

        public override object WzValue => Value;

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Short;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte)2);
            writer.Write(Value);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            Name = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// The value of the property
        /// </summary>
        public short Value { get => val;
            set => val = value;
        }
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
            val = value;
        }
        #endregion

        #region Cast Values
        public override float GetFloat() => val;

        public override double GetDouble() => val;

        public override int GetInt() => val;

        public override short GetShort() => val;

        public override long GetLong() => val;

        public override string ToString() => val.ToString();
        #endregion
    }
}