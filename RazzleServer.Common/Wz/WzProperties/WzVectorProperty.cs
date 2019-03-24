using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that contains an x and a y value
    /// </summary>
    public class WzVectorProperty : WzExtended
    {
        public override void SetValue(object value)
        {
            if (!(value is Point point))
            {
                return;
            }

            X.Value = point.X;
            Y.Value = point.Y;
        }

        public override WzImageProperty DeepClone() => new WzVectorProperty(Name, X, Y);

        public override object WzValue => new Point(X.Value, Y.Value);

        public override WzPropertyType Type => WzPropertyType.Vector;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("Shape2D#Vector2D", 0x73, 0x1B);
            writer.WriteCompressedInt(X.Value);
            writer.WriteCompressedInt(Y.Value);
        }

        public override void Dispose()
        {
            Name = null;
            X.Dispose();
            X = null;
            Y.Dispose();
            Y = null;
        }

        /// <summary>
        /// The X value of the Vector2D
        /// </summary>
        public WzIntProperty X { get; set; }

        /// <summary>
        /// The Y value of the Vector2D
        /// </summary>
        public WzIntProperty Y { get; set; }

        /// <summary>
        /// Creates a blank WzVectorProperty
        /// </summary>
        public WzVectorProperty() { }

        /// <summary>
        /// Creates a WzVectorProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzVectorProperty(string name) => Name = name;

        /// <summary>
        /// Creates a WzVectorProperty with the specified name, x and y
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="x">The x value of the vector</param>
        /// <param name="y">The y value of the vector</param>
        public WzVectorProperty(string name, WzIntProperty x, WzIntProperty y)
        {
            Name = name;
            X = x;
            Y = y;
        }

        public override Point GetPoint() => new Point(X.Value, Y.Value);

        public override string ToString() => $"X: {X.Value}, Y: {Y.Value}";
    }
}
