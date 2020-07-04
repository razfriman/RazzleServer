using System;
using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Wz.WzProperties
{
    /// <summary>
    /// A property that contains several WzExtendedProperties
    /// </summary>
    public class WzConvexProperty : WzExtended, IPropertyContainer
    {
        private List<WzImageProperty> _properties = new List<WzImageProperty>();

        public override WzImageProperty DeepClone()
        {
            var clone = new WzConvexProperty(Name);
            foreach (var prop in _properties)
            {
                clone.AddProperty(prop.DeepClone());
            }

            return clone;
        }

        public override WzPropertyType Type => WzPropertyType.Convex;

        public override WzImageProperty this[string name]
        {
            get => _properties.FirstOrDefault(iwp =>
                String.Equals(iwp.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public WzImageProperty GetProperty(string name)
        {
            return _properties.FirstOrDefault(iwp =>
                String.Equals(iwp.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

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

        public override void Dispose()
        {
            Name = null;
            _properties?.ForEach(x => x.Dispose());
            _properties?.Clear();
            _properties = null;
        }

        /// <summary>
        /// Creates a blank WzConvexProperty
        /// </summary>
        public WzConvexProperty() { }

        /// <summary>
        /// Creates a WzConvexProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzConvexProperty(string name) => Name = name;

        /// <summary>
        /// Adds a WzExtendedProperty to the list of properties
        /// </summary>
        /// <param name="prop">The property to add</param>
        public void AddProperty(WzImageProperty prop)
        {
            if (!(prop is WzExtended))
            {
                throw new ArgumentException($"Property is not {nameof(WzExtended)}");
            }

            prop.Parent = this;
            _properties.Add(prop);
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
            _properties.Remove(prop);
        }

        public void ClearProperties()
        {
            foreach (var prop in _properties)
            {
                prop.Parent = null;
            }

            _properties.Clear();
        }
    }
}
