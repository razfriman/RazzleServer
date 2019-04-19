using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Point = RazzleServer.Common.Util.Point;

namespace RazzleServer.Wz
{
    /// <inheritdoc />
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
        [JsonIgnore]
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
                switch (this)
                {
                    case WzFile wzFile:
                        return wzFile[name];
                    case WzDirectory wzDirectory:
                        return wzDirectory[name];
                    case WzImage wzImage:
                        return wzImage[name];
                    case WzImageProperty wzImageProperty:
                        return wzImageProperty[name];
                    default:
                        return null;
                }
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

        public virtual int GetInt() => throw new NotImplementedException();

        public virtual short GetShort() => throw new NotImplementedException();

        public virtual long GetLong() => throw new NotImplementedException();

        public virtual float GetFloat() => throw new NotImplementedException();

        public virtual double GetDouble() => throw new NotImplementedException();

        public virtual string GetString() => throw new NotImplementedException();

        public virtual Point GetPoint() => throw new NotImplementedException();

        public virtual Bitmap GetBitmap() => throw new NotImplementedException();

        public virtual byte[] GetBytes() => throw new NotImplementedException();

        public void Serialize(string path, bool oneFile = true, JsonSerializer serializer = null)
        {
            if (!oneFile && (this is WzFile || this is WzDirectory))
            {
                switch (this)
                {
                    case WzFile wzFile:
                        wzFile.WzDirectory.Serialize(path, false, serializer);
                        break;
                    case WzDirectory wzDir:
                    {
                        var subPath = Path.Combine(path, wzDir.Name);
                        Directory.CreateDirectory(subPath);
                        wzDir.WzDirectories.ForEach(subDir => subDir.Serialize(subPath, false, serializer));
                        wzDir.WzImages.ForEach(img =>
                            img.Serialize(Path.Combine(subPath, img.Name + ".json"), true, serializer));
                        break;
                    }
                }
            }
            else
            {
                using var stream = File.OpenWrite(path);
                Serialize(stream, serializer);
            }
        }

        public void Serialize(Stream stream, JsonSerializer serializer = null)
        {
            using var sr = new StreamWriter(stream);
            using var writer = new JsonTextWriter(sr);
            serializer ??= new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None
            };

            serializer.Serialize(writer, this);
        }

        public static WzFile DeserializeFile(string contents) => Deserialize<WzFile>(contents);

        public static WzDirectory DeserializeDirectory(string contents) => Deserialize<WzDirectory>(contents);

        public static WzImage DeserializeImage(string contents) => Deserialize<WzImage>(contents);

        public static T Deserialize<T>(string contents) where T : WzObject =>
            JsonConvert.DeserializeObject<T>(contents,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
    }
}
