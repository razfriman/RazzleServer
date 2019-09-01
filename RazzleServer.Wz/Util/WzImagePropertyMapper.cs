using System;
using RazzleServer.Wz.WzProperties;

namespace RazzleServer.Wz.Util
{
    public static class WzImagePropertyMapper
    {
        public static Type GetType(WzPropertyType propertyType)
        {
            return propertyType switch
            {
                WzPropertyType.Null => typeof(WzNullProperty),
                WzPropertyType.Short => typeof(WzShortProperty),
                WzPropertyType.Int => typeof(WzIntProperty),
                WzPropertyType.Long => typeof(WzLongProperty),
                WzPropertyType.Float => typeof(WzFloatProperty),
                WzPropertyType.Double => typeof(WzDoubleProperty),
                WzPropertyType.String => typeof(WzStringProperty),
                WzPropertyType.SubProperty => typeof(WzSubProperty),
                WzPropertyType.Canvas => typeof(WzCanvasProperty),
                WzPropertyType.Vector => typeof(WzVectorProperty),
                WzPropertyType.Convex => typeof(WzConvexProperty),
                WzPropertyType.Sound => typeof(WzSoundProperty),
                WzPropertyType.Uol => typeof(WzUolProperty),
                WzPropertyType.Png => typeof(WzPngProperty),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
            };
        }
    }
}
