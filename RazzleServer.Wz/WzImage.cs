using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Wz.Util;

namespace RazzleServer.Wz
{
    /// <inheritdoc>
    ///     <cref>WzObject</cref>
    /// </inheritdoc>
    /// <summary>
    /// A .img contained in a wz directory
    /// </summary>
    public class WzImage : WzObject, IPropertyContainer
    {
        private Dictionary<string, WzImageProperty> _properties = new Dictionary<string, WzImageProperty>();

        internal WzBinaryReader Reader { get; }

        [JsonIgnore] public bool ParseEverything { get; private set; }
        internal long TempFileStart { get; set; }
        internal long TempFileEnd { get; set; }

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
            Reader = new WzBinaryReader(dataStream, WzTool.GetIvByMapleVersion(mapleVersion));
        }

        internal WzImage(string name, WzBinaryReader reader)
        {
            Name = name;
            Reader = reader;
            BlockStart = (int)reader.BaseStream.Position;
        }

        public override void Dispose()
        {
            Name = null;
            Reader?.Dispose();
            _properties?.Values.ToList().ForEach(x => x.Dispose());
            _properties?.Clear();
            _properties = null;
        }

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

        [JsonIgnore] public int BlockStart { get; }

        public override WzObjectType ObjectType
        {
            get
            {
                if (Reader != null && !Parsed)
                {
                    ParseImage();
                }

                return WzObjectType.Image;
            }
        }

        /// <summary>
        /// The properties contained in the image
        /// </summary>
        public Dictionary<string, WzImageProperty> WzProperties
        {
            get
            {
                if (Reader != null && !Parsed)
                {
                    ParseImage();
                }

                return _properties;
            }
        }

        [JsonIgnore] public IEnumerable<WzImageProperty> WzPropertiesList => WzProperties.Values;

        public WzImage DeepClone()
        {
            if (Reader != null && !Parsed)
            {
                ParseImage();
            }

            var clone = new WzImage(Name) {Changed = true};
            foreach (var prop in _properties.Values)
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
        public WzImageProperty this[string name]
        {
            get
            {
                if (Reader != null && !Parsed)
                {
                    ParseImage();
                }

                return _properties.GetValueOrDefault(name, null);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                value.Name = name;
                AddProperty(value);
            }
        }

        /// <summary>
        /// Gets a WzImageProperty from a path
        /// </summary>
        /// <param name="path">path to object</param>
        /// <returns>the selected WzImageProperty</returns>
        public WzImageProperty GetFromPath(string path)
        {
            if (Reader != null)
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
                var found = (ret == null ? _properties : ret.WzProperties).GetValueOrDefault(segment);

                if (found != null)
                {
                    ret = found;
                }
                else
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
            if (Reader != null && !Parsed)
            {
                ParseImage();
            }

            _properties.Add(prop.Name, prop);
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
            if (Reader != null && !Parsed)
            {
                ParseImage();
            }

            prop.Parent = null;
            _properties.Remove(prop.Name);
        }

        public void ClearProperties()
        {
            foreach (var prop in _properties.Values)
            {
                prop.Parent = null;
            }

            _properties.Clear();
        }

        public override void Remove()
        {
            ((WzDirectory)Parent).RemoveImage(this);
        }

        /// <summary>
        /// Parses the image from the wz file
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

            ParseEverything = parseEverything;
            Reader.BaseStream.Position = Offset;
            var b = Reader.ReadByte();
            if (b != 0x73 || Reader.ReadString() != "Property" || Reader.ReadUInt16() != 0)
            {
                return;
            }

            var parsedProps = WzImageProperty.ParsePropertyList(Offset, Reader, this, this).ToList();
            parsedProps.ForEach(x => _properties.Add(x.Name, x));
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

            ParseEverything = false;
            Reader.BaseStream.Position = Offset;
            var b = Reader.ReadByte();
            if (b != 0x73 || Reader.ReadString() != "Property" || Reader.ReadUInt16() != 0)
            {
                return;
            }

            var parsedProps = WzImageProperty.ParsePropertyList(Offset, Reader, this, this);
            foreach (var parsedProp in parsedProps)
            {
                _properties.Add(parsedProp.Name, parsedProp);
            }

            Parsed = true;
        }

        [JsonIgnore]
        public byte[] DataBlock
        {
            get
            {
                if (Reader == null || BlockSize <= 0)
                {
                    return null;
                }

                var blockData = Reader.ReadBytes(BlockSize);
                Reader.BaseStream.Position = BlockStart;
                return blockData;
            }
        }

        public void UnparseImage()
        {
            Parsed = false;
            _properties.Clear();
        }

        public IEnumerable<string> GetPaths(string curPath)
        {
            var objList = new List<string>();
            foreach (var prop in WzProperties.Values)
            {
                objList.Add(curPath + "/" + prop.Name);
                objList.AddRange(prop.GetPaths(curPath + "/" + prop.Name));
            }

            return objList;
        }
    }
}
