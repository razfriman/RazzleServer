using System;
using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that contains a set of properties
    /// </summary>
    public class WzSubProperty : WzExtended, IPropertyContainer
    {
        #region Fields
        internal List<WzImageProperty> properties = new List<WzImageProperty>();
        internal string name;
        internal WzObject parent;
        //internal WzImage imgParent;
        #endregion

        #region Inherited Members
        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzSubProperty(name);
            foreach (var prop in properties)
            {
                clone.AddProperty(prop.DeepClone());
            }

            return clone;
        }

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override WzObject Parent { get => parent;
            internal set => parent = value;
        }
        /*		/// <summary>
                /// The image that this property is contained in
                /// </summary>
                public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.SubProperty;

        /// <summary>
        /// The wz properties contained in the property
        /// </summary>
        public override List<WzImageProperty> WzProperties => properties;

        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name { get => name;
            set => name = value;
        }
        /// <summary>
        /// Gets a wz property by it's name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>The wz property with the specified name</returns>
        public override WzImageProperty this[string name]
        {
            get
            {

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

        /// <summary>
        /// Gets a wz property by a path name
        /// </summary>
        /// <param name="path">path to property</param>
        /// <returns>the wz property with the specified name</returns>
        public override WzImageProperty GetFromPath(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return ((WzImageProperty)Parent)[path.Substring(name.IndexOf('/') + 1)];
            }
            WzImageProperty ret = this;
            for (var x = 0; x < segments.Length; x++)
            {
                var foundChild = false;
                foreach (var iwp in ret.WzProperties)
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
        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("Property", 0x73, 0x1B);
            WritePropertyList(writer, properties);
        }
        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzSub", Name, true));
            DumpPropertyList(writer, level, WzProperties);
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzSub"));
        }
        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            foreach (var prop in properties)
            {
                prop.Dispose();
            }

            properties.Clear();
            properties = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// Creates a blank WzSubProperty
        /// </summary>
        public WzSubProperty() { }
        /// <summary>
        /// Creates a WzSubProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzSubProperty(string name)
        {
            this.name = name;
        }
        /// <summary>
        /// Adds a property to the list
        /// </summary>
        /// <param name="prop">The property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            prop.Parent = this;
            properties.Add(prop);
        }
        public void AddProperties(List<WzImageProperty> props)
        {
            foreach (var prop in props)
            {
                AddProperty(prop);
            }
        }
        public void RemoveProperty(WzImageProperty prop)
        {
            prop.Parent = null;
            properties.Remove(prop);
        }
        /// <summary>
        /// Clears the list of properties
        /// </summary>
        public void ClearProperties()
        {
            foreach (var prop in properties)
            {
                prop.Parent = null;
            }

            properties.Clear();
        }
        #endregion
    }
}