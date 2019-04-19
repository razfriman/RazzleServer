using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RazzleServer.Wz.Util;
using RazzleServer.Wz.WzProperties;

namespace RazzleServer.Wz
{
    /// <inheritdoc />
    /// <summary>
    /// An interface for wz img properties
    /// </summary>
    [JsonConverter(typeof(WzImagePropertyConverter))]
    public abstract class WzImageProperty : WzObject
    {
        public virtual Dictionary<string, WzImageProperty> WzProperties { get; set; }

        [JsonIgnore] public IEnumerable<WzImageProperty> WzPropertiesList => WzProperties.Values;

        public virtual WzImageProperty this[string name]
        {
            get => WzProperties[name];
            set => throw new NotImplementedException();
        }

        public virtual WzImageProperty GetFromPath(string path) => null;

        [JsonConverter(typeof(StringEnumConverter))]
        public abstract WzPropertyType Type { get; }

        /// <summary>
        /// The image that this property is contained in
        /// </summary>
        [JsonIgnore]
        public WzImage ParentImage
        {
            get
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent is WzImage image)
                    {
                        return image;
                    }

                    parent = parent.Parent;
                }

                return null;
            }
        }

        public override WzObjectType ObjectType => WzObjectType.Property;

        public abstract WzImageProperty DeepClone();

        public override void Remove() => ((IPropertyContainer)Parent)?.RemoveProperty(this);

        public override WzFile WzFileParent => ParentImage.WzFileParent;

        internal static IEnumerable<WzImageProperty> ParsePropertyList(uint offset, WzBinaryReader reader,
            WzObject parent,
            WzImage parentImg)
        {
            var entryCount = reader.ReadCompressedInt();
            var properties = new List<WzImageProperty>(entryCount);
            for (var i = 0; i < entryCount; i++)
            {
                var name = reader.ReadStringBlock(offset);
                var ptype = reader.ReadByte();
                switch (ptype)
                {
                    case 0:
                        properties.Add(new WzNullProperty(name) {Parent = parent});
                        break;
                    case 11:
                    case 2:
                        properties.Add(new WzShortProperty(name, reader.ReadInt16()) {Parent = parent});
                        break;
                    case 3:
                    case 19:
                        properties.Add(new WzIntProperty(name, reader.ReadCompressedInt()) {Parent = parent});
                        break;
                    case 20:
                        properties.Add(new WzLongProperty(name, reader.ReadLong()) {Parent = parent});
                        break;
                    case 4:
                        var type = reader.ReadByte();
                        if (type == 0x80)
                        {
                            properties.Add(new WzFloatProperty(name, reader.ReadSingle()) {Parent = parent});
                        }
                        else if (type == 0)
                        {
                            properties.Add(new WzFloatProperty(name, 0f) {Parent = parent});
                        }

                        break;
                    case 5:
                        properties.Add(new WzDoubleProperty(name, reader.ReadDouble()) {Parent = parent});
                        break;
                    case 8:
                        properties.Add(new WzStringProperty(name, reader.ReadStringBlock(offset)) {Parent = parent});
                        break;
                    case 9:
                        var eob = (int)(reader.ReadUInt32() + reader.BaseStream.Position);
                        WzImageProperty exProp = ParseExtendedProp(reader, offset, name, parent, parentImg);
                        properties.Add(exProp);
                        reader.BaseStream.Position = eob;
                        break;
                    default:
                        throw new Exception("Unknown property type at ParsePropertyList");
                }
            }

            return properties;
        }

        internal static WzExtended ParseExtendedProp(WzBinaryReader reader, uint offset, string name, WzObject parent,
            WzImage imgParent)
        {
            switch (reader.ReadByte())
            {
                case 0x01:
                case 0x1B:
                    return ExtractMore(reader, offset, name, reader.ReadStringAtOffset(offset + reader.ReadInt32()),
                        parent, imgParent);
                case 0x00:
                case 0x73:
                    return ExtractMore(reader, offset, name, "", parent, imgParent);
                default:
                    throw new Exception("Invalid byte read at ParseExtendedProp");
            }
        }

        internal static WzExtended ExtractMore(WzBinaryReader reader, uint offset, string name, string iname,
            WzObject parent, WzImage imgParent)
        {
            if (iname == "")
            {
                iname = reader.ReadString();
            }

            switch (iname)
            {
                case "Property":
                    var subProp = new WzSubProperty(name) {Parent = parent};
                    reader.BaseStream.Position += 2; // Reserved?
                    subProp.AddProperties(ParsePropertyList(offset, reader, subProp, imgParent));
                    return subProp;
                case "Canvas":
                    var canvasProp = new WzCanvasProperty(name) {Parent = parent};
                    reader.BaseStream.Position++;
                    if (reader.ReadByte() == 1)
                    {
                        reader.BaseStream.Position += 2;
                        canvasProp.AddProperties(ParsePropertyList(offset, reader, canvasProp, imgParent));
                    }

                    canvasProp.PngProperty = new WzPngProperty(reader, imgParent.ParseEverything) {Parent = canvasProp};
                    return canvasProp;
                case "Shape2D#Vector2D":
                    var vecProp = new WzVectorProperty(name) {Parent = parent};
                    vecProp.X = new WzIntProperty("X", reader.ReadCompressedInt()) {Parent = vecProp};
                    vecProp.Y = new WzIntProperty("Y", reader.ReadCompressedInt()) {Parent = vecProp};
                    return vecProp;
                case "Shape2D#Convex2D":
                    var convexProp = new WzConvexProperty(name) {Parent = parent};
                    var convexEntryCount = reader.ReadCompressedInt();
                    for (var i = 0; i < convexEntryCount; i++)
                    {
                        convexProp.AddProperty(ParseExtendedProp(reader, offset, name, convexProp, imgParent));
                    }

                    return convexProp;
                case "Sound_DX8":
                    var soundProp = new WzSoundProperty(name, reader, imgParent.ParseEverything) {Parent = parent};
                    return soundProp;
                case "UOL":
                    reader.BaseStream.Position++;
                    switch (reader.ReadByte())
                    {
                        case 0:
                            return new WzUolProperty(name, reader.ReadString()) {Parent = parent};
                        case 1:
                            return new WzUolProperty(name, reader.ReadStringAtOffset(offset + reader.ReadInt32()))
                            {
                                Parent = parent
                            };
                        default:
                            throw new InvalidOperationException("Unsupported UOL type");
                    }

                default:
                    throw new InvalidOperationException("Unknown iname: " + iname);
            }
        }

        public IEnumerable<string> GetPaths(string curPath)
        {
            var objList = new List<string>();
            switch (Type)
            {
                case WzPropertyType.Canvas:
                    foreach (var canvasProp in ((WzCanvasProperty)this).WzProperties.Values)
                    {
                        objList.Add(curPath + "/" + canvasProp.Name);
                        objList.AddRange(canvasProp.GetPaths(curPath + "/" + canvasProp.Name));
                    }

                    objList.Add(curPath + "/PNG");
                    break;
                case WzPropertyType.Convex:
                    foreach (var exProp in ((WzConvexProperty)this).WzProperties.Values)
                    {
                        objList.Add(curPath + "/" + exProp.Name);
                        objList.AddRange(exProp.GetPaths(curPath + "/" + exProp.Name));
                    }

                    break;
                case WzPropertyType.SubProperty:
                    foreach (var subProp in ((WzSubProperty)this).WzProperties.Values)
                    {
                        objList.Add(curPath + "/" + subProp.Name);
                        objList.AddRange(subProp.GetPaths(curPath + "/" + subProp.Name));
                    }

                    break;
                case WzPropertyType.Vector:
                    objList.Add(curPath + "/X");
                    objList.Add(curPath + "/Y");
                    break;
                case WzPropertyType.Null:
                case WzPropertyType.Short:
                case WzPropertyType.Int:
                case WzPropertyType.Long:
                case WzPropertyType.Float:
                case WzPropertyType.Double:
                case WzPropertyType.String:
                case WzPropertyType.Sound:
                case WzPropertyType.Uol:
                case WzPropertyType.Png:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return objList;
        }
    }
}
