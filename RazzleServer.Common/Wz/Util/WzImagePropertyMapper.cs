using System;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Common.Wz.Util
{
    public static class WzImagePropertyMapper
    {
        public static Type GetType(WzPropertyType propertyType)
        {
            switch (propertyType)
            {
                case WzPropertyType.Null:
                    return typeof(WzNullProperty);
                case WzPropertyType.Short:
                    return typeof(WzShortProperty);
                case WzPropertyType.Int:
                    return typeof(WzIntProperty);
                case WzPropertyType.Long:
                    return typeof(WzLongProperty);
                case WzPropertyType.Float:
                    return typeof(WzFloatProperty);
                case WzPropertyType.Double:
                    return typeof(WzDoubleProperty);
                case WzPropertyType.String:
                    return typeof(WzStringProperty);
                case WzPropertyType.SubProperty:
                    return typeof(WzSubProperty);
                case WzPropertyType.Canvas:
                    return typeof(WzCanvasProperty);
                case WzPropertyType.Vector:
                    return typeof(WzVectorProperty);
                case WzPropertyType.Convex:
                    return typeof(WzConvexProperty);
                case WzPropertyType.Sound:
                    return typeof(WzSoundProperty);
                case WzPropertyType.Uol:
                    return typeof(WzUolProperty);
                case WzPropertyType.Png:
                    return typeof(WzPngProperty);
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }
    }
}
