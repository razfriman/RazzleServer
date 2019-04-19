using System;
using System.Collections.Generic;

namespace RazzleServer.Wz.WzProperties
{
    /// <summary>
    /// A property that contains a set of properties
    /// </summary>
    public class WzSubProperty : WzExtended, IPropertyContainer
    {
        public override WzImageProperty DeepClone()
        {
            var clone = new WzSubProperty(Name);
            foreach (var prop in WzProperties.Values)
            {
                clone.AddProperty(prop.DeepClone());
            }

            return clone;
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType Type => WzPropertyType.SubProperty;

        /// <summary>
        /// Gets a wz property by it's name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>The wz property with the specified name</returns>
        public override WzImageProperty this[string name]
        {
            get => WzProperties.GetValueOrDefault(name, null);
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
        /// Gets a wz property by a path name
        /// </summary>
        /// <param name="path">path to property</param>
        /// <returns>the wz property with the specified name</returns>
        public override WzImageProperty GetFromPath(string path)
        {
            var segments = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return ((WzImageProperty)Parent)[path.Substring(Name.IndexOf('/') + 1)];
            }

            WzImageProperty ret = this;
            foreach (var segment in segments)
            {
                var found = ret.WzProperties.GetValueOrDefault(segment);

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
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            Name = null;
            foreach (var prop in WzProperties.Values)
            {
                prop.Dispose();
            }

            WzProperties.Clear();
            WzProperties = null;
        }

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
            Name = name;
        }

        /// <summary>
        /// Adds a property to the list
        /// </summary>
        /// <param name="prop">The property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            prop.Parent = this;
            WzProperties.Add(prop.Name, prop);
        }

        public void AddProperties(IEnumerable<WzImageProperty> props)
        {
            foreach (var prop in props)
            {
                AddProperty(prop);
            }
        }

        public void RemoveProperty(WzImageProperty prop)
        {
            prop.Parent = null;
            WzProperties.Remove(prop.Name);
        }

        /// <summary>
        /// Clears the list of properties
        /// </summary>
        public void ClearProperties()
        {
            foreach (var prop in WzProperties.Values)
            {
                prop.Parent = null;
            }

            WzProperties.Clear();
        }
    }
}
