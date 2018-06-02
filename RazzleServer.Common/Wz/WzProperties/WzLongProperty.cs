using System;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    internal class WzLongProperty : WzImageProperty
    {
        #region Fields
        internal long val;
        #endregion

        #region Inherited Members
        public override void SetValue(object value)
        {
            val = Convert.ToInt64(value);
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzLongProperty(Name, val);
            return clone;
        }

        public override object WzValue => Value;


        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Long;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte)20);
            writer.WriteCompressedLong(Value);
        }
        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzLong", Name, Value.ToString()));
        }
        /// <summary>
        /// Dispose the object
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
        public long Value { get => val;
            set => val = value;
        }
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
            val = value;
        }
        #endregion

        #region Cast Values
        public override float GetFloat()
        {
            return val;
        }

        public override double GetDouble()
        {
            return val;
        }

        public override long GetLong()
        {
            return val;
        }

        public override int GetInt()
        {
            return (int)val;
        }

        public override short GetShort()
        {
            return (short)val;
        }

        public override string ToString()
        {
            return val.ToString();
        }
        #endregion
    }
}