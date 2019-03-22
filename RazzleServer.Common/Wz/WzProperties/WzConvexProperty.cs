using System;
using System.Collections.Generic;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that contains several WzExtendedProperties
    /// </summary>
    public class WzConvexProperty : WzExtended, IPropertyContainer
    {
        #region Fields
        internal List<WzImageProperty> properties = new List<WzImageProperty>();
        #endregion

        #region Inherited Members
        public override void SetValue(object value) => throw new NotImplementedException();

        public override WzImageProperty DeepClone()
        {
            var clone = new WzConvexProperty(Name);
            foreach (var prop in properties)
            {
                clone.AddProperty(prop.DeepClone());
            }

            return clone;
        }

        public override WzPropertyType PropertyType => WzPropertyType.Convex;

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
        }

        public WzImageProperty GetProperty(string name)
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

        public override WzImageProperty GetFromPath(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return ((WzImageProperty)Parent)[path.Substring(Name.IndexOf('/') + 1)];
            }
            WzImageProperty ret = this;
            foreach (var segment in segments)
            {
                var foundChild = false;
                foreach (var iwp in ret.WzProperties)
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
        public override void WriteValue(WzBinaryWriter writer)
        {
            var extendedProps = new List<WzExtended>(properties.Count);
            foreach (var prop in properties)
            {
                if (prop is WzExtended)
                {
                    extendedProps.Add((WzExtended)prop);
                }
            }

            writer.WriteStringValue("Shape2D#Convex2D", 0x73, 0x1B);
            writer.WriteCompressedInt(extendedProps.Count);
            for (var i = 0; i < extendedProps.Count; i++)
            {
                properties[i].WriteValue(writer);
            }
        }

        public override void Dispose()
        {
            Name = null;
            properties?.ForEach(x => x.Dispose());
            properties?.Clear();
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
            Name = name;
        }
        /// <summary>
        /// Adds a WzExtendedProperty to the list of properties
        /// </summary>
        /// <param name="prop">The property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            if (!(prop is WzExtended))
            {
                throw new Exception("Property is not IExtended");
            }

            prop.Parent = this;
            properties.Add((WzExtended)prop);
        }

        public void AddProperties(IEnumerable<WzImageProperty> properties)
        {
            foreach (var property in properties)
            {
                AddProperty(property);
            }
        }

        public void RemoveProperty(WzImageProperty prop)
        {
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

        #endregion
    }
}
