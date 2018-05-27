using System;
using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.Wz.Util;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Common.Wz
{
    /// <summary>
    /// A .img contained in a wz directory
    /// </summary>
    public class WzImage : WzObject, IPropertyContainer
    {
        //TODO: nest wzproperties in a wzsubproperty inside of WzImage

        #region Fields
        internal bool parsed;
        internal string name;
        internal int size, checksum;
        internal uint offset;
        internal WzBinaryReader reader;
        internal List<WzImageProperty> properties = new List<WzImageProperty>();
        internal WzObject parent;
        internal int blockStart;
        internal long tempFileStart;
        internal long tempFileEnd;
        internal bool changed;
        internal bool parseEverything;
        #endregion

        #region Constructors\Destructors
        /// <summary>
        /// Creates a blank WzImage
        /// </summary>
        public WzImage() { }
        /// <summary>
        /// Creates a WzImage with the given name
        /// </summary>
        /// <param name="name">The name of the image</param>
        public WzImage(string name)
        {
            this.name = name;
        }
        public WzImage(string name, Stream dataStream, WzMapleVersion mapleVersion)
        {
            this.name = name;
            reader = new WzBinaryReader(dataStream, WzTool.GetIvByMapleVersion(mapleVersion));
        }
        internal WzImage(string name, WzBinaryReader reader)
        {
            this.name = name;
            this.reader = reader;
            blockStart = (int)reader.BaseStream.Position;
        }

        public override void Dispose()
        {
            name = null;
            reader = null;
			properties?.ForEach(x => x.Dispose());
			properties?.Clear();
			properties = null;
        }
        #endregion

        #region Inherited Members
        /// <summary>
		/// The parent of the object
		/// </summary>
		public override WzObject Parent { get => parent;
            internal set => parent = value;
        }
        /// <summary>
        /// The name of the image
        /// </summary>
        public override string Name { get => name;
            set => name = value;
        }
        public override WzFile WzFileParent => Parent?.WzFileParent;

        /// <summary>
        /// Is the object parsed
        /// </summary>
        public bool Parsed { get => parsed;
            set => parsed = value;
        }
        /// <summary>
        /// Was the image changed
        /// </summary>
        public bool Changed { get => changed;
            set => changed = value;
        }
        /// <summary>
        /// The size in the wz file of the image
        /// </summary>
        public int BlockSize { get => size;
            set => size = value;
        }
        /// <summary>
        /// The checksum of the image
        /// </summary>
        public int Checksum { get => checksum;
            set => checksum = value;
        }
        /// <summary>
        /// The offset of the image
        /// </summary>
        public uint Offset { get => offset;
            set => offset = value;
        }
        public int BlockStart => blockStart;

        /// <summary>
        /// The WzObjectType of the image
        /// </summary>
        public override WzObjectType ObjectType { get { if (reader != null)
            {
                if (!parsed)
                {
                    ParseImage();
                }
            }

            return WzObjectType.Image; } }
        /// <summary>
        /// The properties contained in the image
        /// </summary>
        public List<WzImageProperty> WzProperties
        {
            get
            {
                if (reader != null && !parsed)
                {
                    ParseImage();
                }
                return properties;
            }
        }

        public WzImage DeepClone()
        {
            if (reader != null && !parsed)
            {
                ParseImage();
            }

            var clone = new WzImage(name);
            clone.changed = true;
            foreach (var prop in properties)
            {
                clone.AddProperty(prop.DeepClone());
            }

            return clone;
        }

        /// <summary>
        /// Gets a wz property by it's name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>The wz property with the specified name</returns>
        public new WzImageProperty this[string name]
        {
            get
            {
                if (reader != null)
                {
                    if (!parsed)
                    {
                        ParseImage();
                    }
                }

                foreach (var iwp in properties)
                {
                    if (iwp.Name.ToLower() == name.ToLower())
                    {
                        return iwp;
                    }
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    value.Name = name;
                    AddProperty(value);
                }
            }
        }
        #endregion 

        #region Custom Members
        /// <summary>
		/// Gets a WzImageProperty from a path
		/// </summary>
		/// <param name="path">path to object</param>
		/// <returns>the selected WzImageProperty</returns>
		public WzImageProperty GetFromPath(string path)
        {
            if (reader != null)
            {
                if (!parsed)
                {
                    ParseImage();
                }
            }

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return null;
            }

            WzImageProperty ret = null;
            foreach (var segment in segments)
            {
                var foundChild = false;
                foreach (var iwp in ret == null ? properties : ret.WzProperties)
                {
                    if (iwp.Name == segment)
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

        /// <summary>
        /// Adds a property to the image
        /// </summary>
        /// <param name="prop">Property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            prop.Parent = this;
            if (reader != null && !parsed)
            {
                ParseImage();
            }

            properties.Add(prop);
        }
        public void AddProperties(List<WzImageProperty> props)
        {
            foreach (var prop in props)
            {
                AddProperty(prop);
            }
        }
        /// <summary>
        /// Removes a property by name
        /// </summary>
        /// <param name="prop">The property to remove</param>
        public void RemoveProperty(WzImageProperty prop)
        {
            if (reader != null && !parsed)
            {
                ParseImage();
            }

            prop.Parent = null;
            properties.Remove(prop);
        }
        public void ClearProperties()
        {
            foreach (var prop in properties)
            {
                prop.Parent = null;
            }

            properties.Clear();
        }

        public override void Remove()
        {
            ((WzDirectory)Parent).RemoveImage(this);
        }
        #endregion

        #region Parsing Methods
        /// <summary>
		/// Parses the image from the wz filetod
		/// </summary>
        public void ParseImage(bool parseEverything)
        {
            if (Parsed)
            {
                return;
            }
            if (Changed)
            {
                Parsed = true; return;
            }
            this.parseEverything = parseEverything;
            var originalPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            var b = reader.ReadByte();
            if (b != 0x73 || reader.ReadString() != "Property" || reader.ReadUInt16() != 0)
            {
                return;
            }

            properties.AddRange(WzImageProperty.ParsePropertyList(offset, reader, this, this));
            parsed = true;
        }

        /// <summary>
		/// Parses the image from the wz filetod
		/// </summary>
		public void ParseImage()
        {
            if (Parsed)
            {
                return;
            }
            if (Changed) { Parsed = true; return; }
            parseEverything = false;
            var originalPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            var b = reader.ReadByte();
            if (b != 0x73 || reader.ReadString() != "Property" || reader.ReadUInt16() != 0)
            {
                return;
            }

            properties.AddRange(WzImageProperty.ParsePropertyList(offset, reader, this, this));
            parsed = true;
        }

        public byte[] DataBlock
        {
            get
            {
                byte[] blockData = null;
                if (reader != null && size > 0)
                {
                    blockData = reader.ReadBytes(size);
                    reader.BaseStream.Position = blockStart;
                }
                return blockData;
            }
        }

        public void UnparseImage()
        {
            parsed = false;
            properties = new List<WzImageProperty>();
        }

        internal void SaveImage(WzBinaryWriter writer)
        {
            if (changed)
            {
                if (reader != null && !parsed)
                {
                    ParseImage();
                }

                var imgProp = new WzSubProperty();
                var startPos = writer.BaseStream.Position;
                imgProp.AddProperties(WzProperties);
                imgProp.WriteValue(writer);
                writer.StringCache.Clear();
                size = (int)(writer.BaseStream.Position - startPos);
            }
            else
            {
                var pos = reader.BaseStream.Position;
                reader.BaseStream.Position = offset;
                writer.Write(reader.ReadBytes(size));
                reader.BaseStream.Position = pos;
            }
        }

        public void ExportXml(StreamWriter writer, bool oneFile, int level)
        {
            if (oneFile)
            {
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzImage", name, true));
                WzImageProperty.DumpPropertyList(writer, level, WzProperties);
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzImage"));
            }
            else
            {
                throw new Exception("Under Construction");
            }
        }
        #endregion
    }
}