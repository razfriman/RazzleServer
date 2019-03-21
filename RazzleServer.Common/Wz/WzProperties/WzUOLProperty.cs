using System.Collections.Generic;
using System.Drawing;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;
using Point = RazzleServer.Common.Util.Point;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that's value is a string
    /// </summary>
    public class WzUolProperty : WzExtended
    {
        private static readonly ILogger Log = LogManager.CreateLogger<WzUolProperty>();

        private WzObject _linkVal;

        public override void SetValue(object value) => Value = (string)value;

        public override WzImageProperty DeepClone() => new WzUolProperty(Name, Value) {_linkVal = null};

        public override object WzValue => LinkValue;

        public override List<WzImageProperty> WzProperties =>
            (LinkValue as WzImageProperty)?.WzProperties;

        public override WzImageProperty this[string name] => LinkValue is WzImageProperty property
            ? property[name]
            : (LinkValue as WzImage)?[name];

        public override WzImageProperty GetFromPath(string path) =>
            LinkValue is WzImageProperty property
                ? property.GetFromPath(path)
                : (LinkValue as WzImage)?.GetFromPath(path);

        public override WzPropertyType PropertyType => WzPropertyType.UOL;

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

        public WzObject LinkValue
        {
            get
            {
                if (_linkVal == null)
                {
                    var paths = Value.Split('/');
                    _linkVal = Parent;
                    foreach (var path in paths)
                    {
                        if (path == "..")
                        {
                            _linkVal = _linkVal.Parent;
                        }
                        else
                        {
                            if (_linkVal is WzImageProperty property)
                            {
                                _linkVal = property[path];
                            }
                            else if (_linkVal is WzImage image)
                            {
                                _linkVal = image[path];
                            }
                            else if (_linkVal is WzDirectory directory)
                            {
                                _linkVal = directory[path];
                            }
                            else
                            {
                                Log.LogCritical($"UOL cannot be resolved for property: {FullPath}");
                                return null;
                            }
                        }
                    }
                }

                return _linkVal;
            }
        }

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

        public override int GetInt() => LinkValue.GetInt();

        public override short GetShort() => LinkValue.GetShort();

        public override long GetLong() => LinkValue.GetLong();

        public override float GetFloat() => LinkValue.GetFloat();

        public override double GetDouble() => LinkValue.GetDouble();

        public override string GetString() => LinkValue.GetString();

        public override Point GetPoint() => LinkValue.GetPoint();

        public override Bitmap GetBitmap() => LinkValue.GetBitmap();

        public override byte[] GetBytes() => LinkValue.GetBytes();

        public override string ToString() => Value;
    }
}
