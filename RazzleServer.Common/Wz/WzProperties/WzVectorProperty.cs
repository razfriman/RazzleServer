using System.IO;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
	/// <summary>
	/// A property that contains an x and a y value
	/// </summary>
	public class WzVectorProperty : WzExtended
	{
		#region Fields
		internal WzIntProperty x, y;
		#endregion

		#region Inherited Members
        public override void SetValue(object value)
        {
            if (value is Point)
            {
                x.val = ((Point)value).X;
                y.val = ((Point)value).Y;
            }
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzVectorProperty(Name, x, y);
            return clone;
        }

		public override object WzValue => new Point(x.Value, y.Value);

		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.Vector;

		public override void WriteValue(WzBinaryWriter writer)
		{
			writer.WriteStringValue("Shape2D#Vector2D", 0x73, 0x1B);
			writer.WriteCompressedInt(X.Value);
			writer.WriteCompressedInt(Y.Value);
		}
		public override void ExportXml(StreamWriter writer, int level)
		{
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzVector", Name, false) +
				XmlUtil.Attrib("X", X.Value.ToString()) + XmlUtil.Attrib("Y", Y.Value.ToString(), true, true));
		}
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
            Name = null;
			x.Dispose();
			x = null;
			y.Dispose();
			y = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// The X value of the Vector2D
		/// </summary>
		public WzIntProperty X { get => x;
			set => x = value;
		}
		/// <summary>
		/// The Y value of the Vector2D
		/// </summary>
		public WzIntProperty Y { get => y;
			set => y = value;
		}
		/// <summary>
		/// The Point of the Vector2D created from the X and Y
		/// </summary>
		public Point Pos => new Point(X.Value, Y.Value);

		/// <summary>
		/// Creates a blank WzVectorProperty
		/// </summary>
		public WzVectorProperty() { }
		/// <summary>
		/// Creates a WzVectorProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		public WzVectorProperty(string name)
		{
            Name = name;
		}
		/// <summary>
		/// Creates a WzVectorProperty with the specified name, x and y
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="x">The x value of the vector</param>
		/// <param name="y">The y value of the vector</param>
		public WzVectorProperty(string name, WzIntProperty x, WzIntProperty y)
		{
            Name = name;
			this.x = x;
			this.y = y;
		}
		#endregion

        #region Cast Values
        public override Point GetPoint()
        {
            return new Point(x.val, y.val);
        }

        public override string ToString()
        {
            return "X: " + x.val + ", Y: " + y.val;
        }
        #endregion
	}
}