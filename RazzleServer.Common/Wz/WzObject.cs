using System;
using System.DrawingCore;
using System.IO;
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

        [JsonIgnore]
        public virtual object WzValue => null;

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

        public void Export(string path)
        {
            using (var s = File.OpenWrite(path))
            using (var sr = new StreamWriter(s))
            using (var writer = new JsonTextWriter(sr))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, this);
            }
        }
    }
}