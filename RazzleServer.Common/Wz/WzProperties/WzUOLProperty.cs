#define UOLRES

using System.Collections.Generic;
using System.DrawingCore;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;
using Point = RazzleServer.Common.Util.Point;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that's value is a string
    /// </summary>
    public class WzUOLProperty : WzExtended
    {
        private static ILogger Log = LogManager.Log;

        #region Fields
        internal string val;
        internal WzObject linkVal;
        #endregion

        #region Inherited Members
        public override void SetValue(object value)
        {
            val = (string)value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzUOLProperty(Name, val) {linkVal = null};
            return clone;
        }

        public override object WzValue
        {
            get
            {
#if UOLRES
                return LinkValue;
#else
                return this;
#endif
            }
        }

#if UOLRES

        public override List<WzImageProperty> WzProperties => LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue).WzProperties : null;

        public override WzImageProperty this[string name] => LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue)[name] : LinkValue is WzImage ? ((WzImage)LinkValue)[name] : null;

        public override WzImageProperty GetFromPath(string path)
        {
            return LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue).GetFromPath(path) : LinkValue is WzImage ? ((WzImage)LinkValue).GetFromPath(path) : null;
        }
#endif

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.UOL;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("UOL", 0x73, 0x1B);
            writer.Write((byte)0);
            writer.WriteStringValue(Value, 0, 1);
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            Name = null;
            val = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// The value of the property
        /// </summary>
        public string Value { get => val;
            set => val = value;
        }

#if UOLRES
        public WzObject LinkValue
        {
            get
            {
                if (linkVal == null)
                {
                    var paths = val.Split('/');
                    linkVal = Parent;
                    var asdf = Parent.FullPath;
                    foreach (var path in paths)
                    {
                        if (path == "..")
                        {
                            linkVal = linkVal.Parent;
                        }
                        else
                        {
                            if (linkVal is WzImageProperty)
                            {
                                linkVal = ((WzImageProperty)linkVal)[path];
                            }
                            else if (linkVal is WzImage)
                            {
                                linkVal = ((WzImage)linkVal)[path];
                            }
                            else if (linkVal is WzDirectory)
                            {
                                linkVal = ((WzDirectory)linkVal)[path];
                            }
                            else
                            {
                                Log.LogCritical($"UOL got nexon'd at property: {FullPath}");
                                return null;
                            }
                        }
                    }
                }
                return linkVal;
            }
        }
#endif

        /// <summary>
        /// Creates a blank WzUOLProperty
        /// </summary>
        public WzUOLProperty() { }

        /// <summary>
        /// Creates a WzUOLProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzUOLProperty(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a WzUOLProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzUOLProperty(string name, string value)
        {
            Name = name;
            val = value;
        }
        #endregion

        #region Cast Values
#if UOLRES
        public override int GetInt()
        {
            return LinkValue.GetInt();
        }

        public override short GetShort()
        {
            return LinkValue.GetShort();
        }

        public override long GetLong()
        {
            return LinkValue.GetLong();
        }

        public override float GetFloat()
        {
            return LinkValue.GetFloat();
        }

        public override double GetDouble()
        {
            return LinkValue.GetDouble();
        }

        public override string GetString()
        {
            return LinkValue.GetString();
        }

        public override Point GetPoint()
        {
            return LinkValue.GetPoint();
        }

        public override Bitmap GetBitmap()
        {
            return LinkValue.GetBitmap();
        }

        public override byte[] GetBytes()
        {
            return LinkValue.GetBytes();
        }
#else
        public override string GetString()
        {
            return val;
        }
#endif
        public override string ToString()
        {
            return val;
        }
        #endregion
    }
}