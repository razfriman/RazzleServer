using System;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that is stored in the wz file with a signed byte and possibly followed by an int. If the 
    /// signed byte is equal to -128, the value is is the int that follows, else the value is the byte.
    /// </summary>
    public class WzIntProperty : WzImageProperty
    {
        #region Fields
        internal string name;
        internal int val;
        internal WzObject parent;
        //internal WzImage imgParent;
        #endregion

        #region Inherited Members
        public override void SetValue(object value)
        {
            val = Convert.ToInt32(value);
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzIntProperty(name, val);
            return clone;
        }

        public override object WzValue => Value;

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override WzObject Parent { get => parent;
            internal set => parent = value;
        }
        /*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Int;

        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name { get => name;
            set => name = value;
        }
        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte)3);
            writer.WriteCompressedInt(Value);
        }
        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzCompressedInt", Name, Value.ToString()));
        }
        /// <summary>
        /// Dispose the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// The value of the property
        /// </summary>
        public int Value { get => val;
            set => val = value;
        }
        /// <summary>
        /// Creates a blank WzCompressedIntProperty
        /// </summary>
        public WzIntProperty() { }
        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzIntProperty(string name)
        {
            this.name = name;
        }
        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzIntProperty(string name, int value)
        {
            this.name = name;
            val = value;
        }
        #endregion

        #region Cast Values
        public override float GetFloat() => val;

        public override double GetDouble() => val;

        public override int GetInt() => val;

        public override short GetShort() => (short)val;

        public override long GetLong() => val;

        public override string ToString() => val.ToString();
        #endregion
    }
}