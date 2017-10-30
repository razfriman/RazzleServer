using System;
using System.Collections.Generic;
using System.IO;
using RazzleServer.Common.WzLib.Util;

namespace RazzleServer.Common.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains several WzExtendedPropertys
    /// </summary>
    public class WzConvexProperty : WzExtended, IPropertyContainer
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
            WzConvexProperty clone = new WzConvexProperty(name);
            foreach (WzImageProperty prop in properties)
                clone.AddProperty(prop.DeepClone());
            return clone;
        }

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override WzObject Parent { get { return parent; } internal set { parent = value; } }
        /*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType { get { return WzPropertyType.Convex; } }
        /// <summary>
        /// The properties contained in the property
        /// </summary>
        public override List<WzImageProperty> WzProperties
        {
            get
            {
                return properties; //properties.ConvertAll<IWzImageProperty>(new Converter<IExtended, IWzImageProperty>(delegate(IExtended source) { return (IWzImageProperty)source; }));
            }
        }
        /// <summary>
        /// The name of this property
        /// </summary>
        public override string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// Gets a wz property by it's name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>The wz property with the specified name</returns>
        public override WzImageProperty this[string name]
        {
            get
            {
                foreach (WzImageProperty iwp in properties)
                    if (iwp.Name.ToLower() == name.ToLower())
                        return iwp;
                //throw new KeyNotFoundException("A wz property with the specified name was not found");
                return null;
            }
        }

        public WzImageProperty GetProperty(string name)
        {
            foreach (WzImageProperty iwp in properties)
                if (iwp.Name.ToLower() == name.ToLower())
                    return iwp;
            return null;
        }

        /// <summary>
        /// Gets a wz property by a path name
        /// </summary>
        /// <param name="path">path to property</param>
        /// <returns>the wz property with the specified name</returns>
        public override WzImageProperty GetFromPath(string path)
        {
            string[] segments = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return ((WzImageProperty)Parent)[path.Substring(name.IndexOf('/') + 1)];
            }
            WzImageProperty ret = this;
            for (int x = 0; x < segments.Length; x++)
            {
                bool foundChild = false;
                foreach (WzImageProperty iwp in ret.WzProperties)
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
        public override void WriteValue(RazzleServer.Common.WzLib.Util.WzBinaryWriter writer)
        {
            List<WzExtended> extendedProps = new List<WzExtended>(properties.Count);
            foreach (WzImageProperty prop in properties) if (prop is WzExtended) extendedProps.Add((WzExtended)prop);
            writer.WriteStringValue("Shape2D#Convex2D", 0x73, 0x1B);
            writer.WriteCompressedInt(extendedProps.Count);
            for (int i = 0; i < extendedProps.Count; i++)
            {
                properties[i].WriteValue(writer);
            }
        }
        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzConvex", this.Name, true));
            WzImageProperty.DumpPropertyList(writer, level, WzProperties);
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzConvex"));
        }
        public override void Dispose()
        {
            name = null;
            properties?.ForEach(x => x.Dispose());
            properties.Clear();
            properties = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// Creates a blank WzConvexProperty
        /// </summary>
        public WzConvexProperty() { }
        /// <summary>
        /// Creates a WzConvexProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzConvexProperty(string name)
        {
            this.name = name;
        }
        /// <summary>
        /// Adds a WzExtendedProperty to the list of properties
        /// </summary>
        /// <param name="prop">The property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            if (!(prop is WzExtended))
                throw new Exception("Property is not IExtended");
            prop.Parent = this;
            properties.Add((WzExtended)prop);
        }

        public void AddProperties(List<WzImageProperty> properties)
        {
            foreach (WzImageProperty property in properties)
                AddProperty(property);
        }

        public void RemoveProperty(WzImageProperty prop)
        {
            prop.Parent = null;
            properties.Remove(prop);
        }

        public void ClearProperties()
        {
            foreach (WzImageProperty prop in properties) prop.Parent = null;
            properties.Clear();
        }

        #endregion
    }
}