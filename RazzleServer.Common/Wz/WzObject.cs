using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Point = RazzleServer.Common.Util.Point;

namespace RazzleServer.Common.Wz
{
    /// <summary>
    /// An abstract class for wz objects
    /// </summary>
    public abstract class WzObject : IDisposable
    {
        /// <summary>
        /// Returns the parent object
        /// </summary>
        [JsonIgnore]
        public WzObject Parent { get; internal set; }

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The WzObjectType of the object
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract WzObjectType ObjectType { get; }

        /// <summary>
        /// Returns the parent WZ File
        /// </summary>
        [JsonIgnore]
        public abstract WzFile WzFileParent { get; }

        public abstract void Dispose();

        public abstract void Remove();

        public WzObject this[string name]
        {
            get
            {
                if (this is WzFile wzFile)
                {
                    return wzFile[name];
                }

                if (this is WzDirectory wzDirectory)
                {
                    return wzDirectory[name];
                }

                if (this is WzImage wzImage)
                {
                    return wzImage[name];
                }

                if (this is WzImageProperty wzImageProperty)
                {
                    return wzImageProperty[name];
                }

                return null;
            }
        }

        [JsonIgnore]
        public string FullPath
        {
            get
            {
                if (this is WzFile wzFile)
                {
                    return wzFile.WzDirectory.Name;
                }

                var result = Name;
                var currObj = this;
                while (currObj.Parent != null)
                {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }

                return result;
            }
        }

        [JsonIgnore] public virtual object WzValue => null;

        #region Cast Values

        public virtual int GetInt()
        {
            throw new NotImplementedException();
        }

        public virtual short GetShort()
        {
            throw new NotImplementedException();
        }

        public virtual long GetLong()
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat()
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual Point GetPoint()
        {
            throw new NotImplementedException();
        }

        public virtual Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        #endregion

        public virtual IEnumerable<WzObject> GetObjects() => Enumerable.Empty<WzObject>();


        public void Export(string path, JsonSerializer serializer = null)
        {
            using (var stream = File.OpenWrite(path))
            {
                Export(stream, serializer);
            }
        }

        public void Export(Stream stream, JsonSerializer serializer = null)
        {
            using (var sr = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(sr))
            {
                serializer = serializer ?? new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, this);
            }
        }
    }
}