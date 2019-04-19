using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RazzleServer.Wz.WzProperties
{
    /// <summary>
    /// A property that can contain sub properties and has one png image
    /// </summary>
    public class WzCanvasProperty : WzExtended, IPropertyContainer
    {
        public override void SetValue(object value) => PngProperty = (WzPngProperty)value;

        public override WzImageProperty DeepClone()
        {
            var clone = new WzCanvasProperty(Name);
            foreach (var prop in WzProperties.Values)
            {
                clone.AddProperty(prop.DeepClone());
            }

            clone.PngProperty = (WzPngProperty)PngProperty.DeepClone();
            return clone;
        }

        public override object WzValue => PngProperty;

        public override WzPropertyType Type => WzPropertyType.Canvas;

        public override WzImageProperty this[string name]
        {
            get => name == "PNG" ? PngProperty : WzProperties.GetValueOrDefault(name, null);
            set
            {
                if (value == null)
                {
                    return;
                }

                if (name == "PNG")
                {
                    PngProperty = (WzPngProperty)value;
                    return;
                }

                value.Name = name;
                AddProperty(value);
            }
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
                if (segment == "PNG")
                {
                    return PngProperty;
                }
                
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
        /// Dispose the object
        /// </summary>
        public override void Dispose()
        {
            Name = null;
            PngProperty.Dispose();
            PngProperty = null;
            WzProperties?.Values?.ToList().ForEach(x => x.Dispose());
            WzProperties?.Clear();
            WzProperties = null;
        }

        /// <summary>
        /// The png image for this canvas property
        /// </summary>
        public WzPngProperty PngProperty { get; set; }

        /// <summary>
        /// Creates a blank WzCanvasProperty
        /// </summary>
        public WzCanvasProperty() { }

        /// <summary>
        /// Creates a WzCanvasProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzCanvasProperty(string name) => Name = name;

        /// <summary>
        /// Adds a property to the property list of this property
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

        /// <summary>
        /// Remove a property
        /// </summary>
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

        public override Bitmap GetBitmap() => PngProperty.GetPng(false);
    }
}
