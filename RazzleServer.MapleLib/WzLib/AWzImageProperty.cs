using System;
using System.IO;
using System.Collections.Generic;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib
{
	/// <summary>
	/// An interface for wz img properties
	/// </summary>
	public abstract class AWzImageProperty : AWzObject
	{
        public override WzObjectType ObjectType { get { return WzObjectType.Property; } }
        
        public abstract WzPropertyType PropertyType { get; }

		public abstract WzImage ParentImage { get; internal set; }

		public abstract void WriteValue(WzBinaryWriter pWriter);

		public virtual void ExportXml(StreamWriter pWriter, int pLevel)
		{
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.OpenNamedTag(this.PropertyType.ToString(), this.Name, true));
			pWriter.WriteLine(XmlUtil.Indentation(pLevel) + XmlUtil.CloseTag(this.PropertyType.ToString()));
		}

		public virtual List<AWzImageProperty> WzProperties { get { return null; } }

        public virtual AWzImageProperty this[string pName] { get { return null; } }

		/// <summary>
		/// Gets a wz property by a path name
		/// </summary>
		/// <param name="pPath">path to property</param>
		/// <returns>the wz property with the specified name</returns>
		public virtual AWzImageProperty GetFromPath(string pPath)
		{
			string[] segments = pPath.Split(new char[1] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
			if (segments[0] == "..")
			{
				return ((AWzImageProperty)Parent)[pPath.Substring(Name.IndexOf('/') + 1)];
			}
			AWzImageProperty ret = this;
            if (ret.WzProperties == null)
            {
                return null;
            }
			for (int x = 0; x < segments.Length; x++)
			{
				bool foundChild = false;
                if (ret is WzPngProperty && segments[x] == "PNG")
                {
                    return ret.ToPngProperty(null);
                }
				foreach (AWzImageProperty iwp in ret.WzProperties)
				{
                    
                    if (iwp.Name == segments[x])
					{
						ret = iwp;
						foundChild = true;
						break;
					}
				}
				if (!foundChild)
				{
					return null;
				}
			}
			return ret;
		}


		internal static void WritePropertyList(WzBinaryWriter pWriter, List<AWzImageProperty> pProperties)
		{
			pWriter.Write((ushort)0);
            pWriter.WriteCompressedInt(pProperties.Count);
			foreach (AWzImageProperty prop in pProperties)
			{
				pWriter.WriteStringValue(prop.Name, 0x00, 0x01);
                if (prop is IExtended)
                {
                    WriteExtendedProperty(pWriter, prop);
                }
                else
                {
                    prop.WriteValue(pWriter);
                }
			}
		}

        internal static void WriteExtendedProperty(WzBinaryWriter pWriter, AWzImageProperty pProp)
        {
            pWriter.Write((byte)9);
            long beforePos = pWriter.BaseStream.Position;
            pWriter.Write(0); // Placeholder
            pProp.WriteValue(pWriter);
            int len = (int)(pWriter.BaseStream.Position - beforePos);
            long newPos = pWriter.BaseStream.Position;
            pWriter.BaseStream.Position = beforePos;
            pWriter.Write(len - 4);
            pWriter.BaseStream.Position = newPos;
        }

		internal static void DumpPropertyList(StreamWriter pWriter, int pLevel, List<AWzImageProperty> pProperties)
		{
			foreach (AWzImageProperty prop in pProperties)
			{
				prop.ExportXml(pWriter, pLevel + 1);
			}
		}

		internal static List<AWzImageProperty> ParsePropertyList(uint pOffset, WzBinaryReader pReader, AWzObject pParent, WzImage pParentImg)
		{
			List<AWzImageProperty> properties = new List<AWzImageProperty>();
			int entryCount = pReader.ReadCompressedInt();
			for (int i = 0; i < entryCount; i++)
			{
				string name = pReader.ReadStringBlock(pOffset);
                byte b = pReader.ReadByte();
				switch (b)
				{
					case 0:
						properties.Add(new WzNullProperty(name) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 0x0B:
					case 2:
						properties.Add(new WzUnsignedShortProperty(name, pReader.ReadUInt16()) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 3:
						properties.Add(new WzCompressedIntProperty(name, pReader.ReadCompressedInt()) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 4:
						byte type = pReader.ReadByte();
						if (type == 0x80)
							properties.Add(new WzByteFloatProperty(name, pReader.ReadSingle()) { Parent = pParent, ParentImage = pParentImg });
						else if (type == 0)
							properties.Add(new WzByteFloatProperty(name, 0f) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 5:
						properties.Add(new WzDoubleProperty(name, pReader.ReadDouble()) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 8:
						properties.Add(new WzStringProperty(name, pReader.ReadStringBlock(pOffset)) { Parent = pParent, ParentImage = pParentImg });
						break;
					case 9:
						int eob = (int)(pReader.ReadUInt32() + pReader.BaseStream.Position);
                        AWzImageProperty exProp = ParseExtendedProp(pReader, pOffset, eob, name, pParent, pParentImg);
						properties.Add(exProp);
						if (pReader.BaseStream.Position != eob) pReader.BaseStream.Position = eob;
						break;
                    default:
                        throw new Exception("Unknown property type at ParsePropertyList: " + b);
				}
			}
            return properties;
        }

        internal static AWzImageProperty ParseExtendedProp(WzBinaryReader pReader, uint pOffset, int pEndOfBlock, string pName, AWzObject pParent, WzImage pImgParent)
		{
            byte b = pReader.ReadByte();
			switch (b)
			{
				case 0x1B:
                    return ExtractMore(pReader, pOffset, pEndOfBlock, pName, pReader.ReadStringAtOffset(pOffset + pReader.ReadInt32()), pParent, pImgParent);
				case 0x73:
                    return ExtractMore(pReader, pOffset, pEndOfBlock, pName, pReader.ReadString(), pParent, pImgParent);
                default:
                    throw new System.Exception("Invlid type at ParseExtendedProp: " + b);
			}
		}
        internal static AWzImageProperty ExtractMore(WzBinaryReader pReader, uint pOffset, int pEndOfBlock, string pName, string pImageName, AWzObject pParent, WzImage pImgParent)
        {
            switch (pImageName)
            {
                case "Property":
                    WzSubProperty subProp = new WzSubProperty(pName) { Parent = pParent, ParentImage = pImgParent };
                    pReader.BaseStream.Position += 2;
                    subProp.AddProperties(AWzImageProperty.ParsePropertyList(pOffset, pReader, subProp, pImgParent));
                    return subProp;
                case "Canvas":
                    WzCanvasProperty canvasProp = new WzCanvasProperty(pName) { Parent = pParent, ParentImage = pImgParent };
                    pReader.BaseStream.Position++;
                    if (pReader.ReadByte() == 1)
                    {
                        pReader.BaseStream.Position += 2;
                        canvasProp.AddProperties(AWzImageProperty.ParsePropertyList(pOffset, pReader, canvasProp, pImgParent));
                    }
                    canvasProp.PngProperty = new WzPngProperty(pReader) { Parent = canvasProp, ParentImage = pImgParent };
                    return canvasProp;
                case "Shape2D#Vector2D":
                    WzVectorProperty vecProp = new WzVectorProperty(pName) { Parent = pParent, ParentImage = pImgParent };
                    vecProp.X = new WzCompressedIntProperty("X", pReader.ReadCompressedInt()) { Parent = vecProp, ParentImage = pImgParent };
                    vecProp.Y = new WzCompressedIntProperty("Y", pReader.ReadCompressedInt()) { Parent = vecProp, ParentImage = pImgParent };
                    return vecProp;
                case "Shape2D#Convex2D":
                    WzConvexProperty convexProp = new WzConvexProperty(pName) { Parent = pParent, ParentImage = pImgParent };
                    int convexEntryCount = pReader.ReadCompressedInt();
                    for (int i = 0; i < convexEntryCount; i++)
                    {
                        convexProp.AddProperty(ParseExtendedProp(pReader, pOffset, 0, pName, convexProp, pImgParent));
                    }
                    return convexProp;
                case "Sound_DX8":
                    WzSoundProperty soundProp = new WzSoundProperty(pName) { Parent = pParent, ParentImage = pImgParent };
                    soundProp.ParseSound(pReader);
                    return soundProp;
                case "UOL":
                    pReader.BaseStream.Position++;
                    byte b = pReader.ReadByte();
                    switch (b)
                    {
                        case 0:
                            return new WzUOLProperty(pName, pReader.ReadString()) { Parent = pParent, ParentImage = pImgParent };
                        case 1:
                            return new WzUOLProperty(pName, pReader.ReadStringAtOffset(pOffset + pReader.ReadInt32())) { Parent = pParent, ParentImage = pImgParent };
                        default:
                            throw new Exception("Unsupported UOL type: " + b);
                    }
                default:
                    throw new Exception("Unknown image name: " + pImageName);
            }
        }
	}
}