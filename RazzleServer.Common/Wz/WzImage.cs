using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RazzleServer.Common.Wz.Util;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Common.Wz
{
    /// <summary>
    /// A .img contained in a wz directory
    /// </summary>
    public class WzImage : WzObject, IPropertyContainer
    {
        #region Fields

        internal WzBinaryReader reader;
        internal List<WzImageProperty> properties = new List<WzImageProperty>();
        internal int blockStart;
        internal long tempFileStart;
        internal long tempFileEnd;
        internal bool parseEverything;

        #endregion

        #region Constructors\Destructors

        /// <summary>
        /// Creates a blank WzImage
        /// </summary>
        public WzImage()
        {
        }

        /// <summary>
        /// Creates a WzImage with the given name
        /// </summary>
        /// <param name="name">The name of the image</param>
        public WzImage(string name)
        {
            Name = name;
        }

        public WzImage(string name, Stream dataStream, WzMapleVersionType mapleVersion)
        {
            Name = name;
            reader = new WzBinaryReader(dataStream, WzTool.GetIvByMapleVersion(mapleVersion));
        }

        internal WzImage(string name, WzBinaryReader reader)
        {
            Name = name;
            this.reader = reader;
            blockStart = (int) reader.BaseStream.Position;
        }

        public override void Dispose()
        {
            Name = null;
            reader = null;
            properties?.ForEach(x => x.Dispose());
            properties?.Clear();
            properties = null;
        }

        #endregion

        #region Inherited Members

        public override WzFile WzFileParent => Parent?.WzFileParent;

        /// <summary>
        /// Is the object Parsed
        /// </summary>
        [JsonIgnore]
        public bool Parsed { get; set; }

        /// <summary>
        /// Was the image changed
        /// </summary>
        [JsonIgnore]
        public bool Changed { get; set; }

        /// <summary>
        /// The size in the wz file of the image
        /// </summary>
        [JsonIgnore]
        public int BlockSize { get; set; }

        /// <summary>
        /// The checksum of the image
        /// </summary>
        [JsonIgnore]
        public int Checksum { get; set; }

        /// <summary>
        /// The Offset of the image
        /// </summary>
        [JsonIgnore]
        public uint Offset { get; set; }

        [JsonIgnore] public int BlockStart => blockStart;

        /// <summary>
        /// The WzObjectType of the image
        /// </summary>
        public override WzObjectType ObjectType
        {
            get
            {
                if (reader != null)
                {
                    if (!Parsed)
                    {
                        ParseImage();
                    }
                }

                return WzObjectType.Image;
            }
        }

        /// <summary>
        /// The properties contained in the image
        /// </summary>
        public List<WzImageProperty> WzProperties
        {
            get
            {
                if (reader != null && !Parsed)
                {
                    ParseImage();
                }

                return properties;
            }
        }

        public WzImage DeepClone()
        {
            if (reader != null && !Parsed)
            {
                ParseImage();
            }

            var clone = new WzImage(Name) {Changed = true};
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
                    if (!Parsed)
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
                if (!Parsed)
                {
                    ParseImage();
                }
            }

            var segments = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
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
            if (reader != null && !Parsed)
            {
                ParseImage();
            }

            properties.Add(prop);
        }

        public void AddProperties(IEnumerable<WzImageProperty> props)
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
            if (reader != null && !Parsed)
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
            ((WzDirectory) Parent).RemoveImage(this);
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
                Parsed = true;
                return;
            }

            this.parseEverything = parseEverything;
            reader.BaseStream.Position = Offset;
            var b = reader.ReadByte();
            if (b != 0x73 || reader.ReadString() != "Property" || reader.ReadUInt16() != 0)
            {
                return;
            }

            properties.AddRange(WzImageProperty.ParsePropertyList(Offset, reader, this, this));
            Parsed = true;
        }

        /// <summary>
        /// Parses the image from the wz file
        /// </summary>
        public void ParseImage()
        {
            if (Parsed)
            {
                return;
            }

            if (Changed)
            {
                Parsed = true;
                return;
            }

            parseEverything = false;
            reader.BaseStream.Position = Offset;
            var b = reader.ReadByte();
            if (b != 0x73 || reader.ReadString() != "Property" || reader.ReadUInt16() != 0)
            {
                return;
            }

            properties.AddRange(WzImageProperty.ParsePropertyList(Offset, reader, this, this));
            Parsed = true;
        }

        [JsonIgnore]
        public byte[] DataBlock
        {
            get
            {
                byte[] blockData = null;
                if (reader != null && BlockSize > 0)
                {
                    blockData = reader.ReadBytes(BlockSize);
                    reader.BaseStream.Position = blockStart;
                }

                return blockData;
            }
        }

        public void UnparseImage()
        {
            Parsed = false;
            properties = new List<WzImageProperty>();
        }

        internal void SaveImage(WzBinaryWriter writer)
        {
            if (Changed)
            {
                if (reader != null && !Parsed)
                {
                    ParseImage();
                }

                var imgProp = new WzSubProperty();
                var startPos = writer.BaseStream.Position;
                imgProp.AddProperties(WzProperties);
                imgProp.WriteValue(writer);
                writer.StringCache.Clear();
                BlockSize = (int) (writer.BaseStream.Position - startPos);
            }
            else
            {
                var pos = reader.BaseStream.Position;
                reader.BaseStream.Position = Offset;
                writer.Write(reader.ReadBytes(BlockSize));
                reader.BaseStream.Position = pos;
            }
        }

        public override IEnumerable<WzObject> GetObjects()
        {
            var objList = new List<WzObject>();
            foreach (var prop in WzProperties)
            {
                objList.Add(prop);
                objList.AddRange(prop.GetObjects());
            }
            return objList;
        }

        #endregion
    }
}
