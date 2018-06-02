using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using RazzleServer.Common.Wz.Util;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Common.Wz
{
    public abstract class ProgressingWzSerializer
    {
        public int Total { get; internal set; }
        public int Current { get; internal set; }

        protected static void CreateDirSafe(ref string path)
        {
            if (path.Substring(path.Length - 1, 1) == @"\")
            {
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }

            var basePath = path;
            var curridx = 0;
            while (Directory.Exists(path) || File.Exists(path))
            {
                curridx++;
                path = basePath + curridx;
            }
            Directory.CreateDirectory(path);
        }
    }

    public abstract class WzXmlSerializer : ProgressingWzSerializer
    {
        protected string Indent;
        protected string LineBreak;
        public static NumberFormatInfo FormattingInfo;
        protected bool ExportBase64Data;

        protected static char[] Amp = "&amp;".ToCharArray();
        protected static char[] Lt = "&lt;".ToCharArray();
        protected static char[] Gt = "&gt;".ToCharArray();
        protected static char[] Apos = "&apos;".ToCharArray();
        protected static char[] Quot = "&quot;".ToCharArray();

        static WzXmlSerializer()
        {
            FormattingInfo = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ","
            };
        }

        internal WzXmlSerializer(int indentation, LineBreak lineBreakType)
        {
            switch (lineBreakType)
            {
                case Wz.LineBreak.None:
                    LineBreak = "";
                    break;
                case Wz.LineBreak.Windows:
                    LineBreak = "\r\n";
                    break;
                case Wz.LineBreak.Unix:
                    LineBreak = "\n";
                    break;
            }
            var indentArray = new char[indentation];
            for (var i = 0; i < indentation; i++)
            {
                {
                    indentArray[i] = (char)0x20;
                }
            }

            Indent = new string(indentArray);
        }

        protected void WritePropertyToXml(TextWriter tw, string depth, WzImageProperty prop)
        {
            if (prop is WzCanvasProperty property3)
            {
                if (ExportBase64Data)
                {
                    var stream = new MemoryStream();
                    property3.PngProperty.GetPNG(false).Save(stream, ImageFormat.Png);
                    var pngbytes = stream.ToArray();
                    stream.Close();
                    tw.Write(string.Concat(depth, "<canvas name=\"", XmlUtil.SanitizeText(property3.Name), "\" width=\"", property3.PngProperty.Width, "\" height=\"", property3.PngProperty.Height, "\" basedata=\"", Convert.ToBase64String(pngbytes), "\">") + LineBreak);
                }
                else
                {
                    {
                        tw.Write(string.Concat(depth, "<canvas name=\"", XmlUtil.SanitizeText(property3.Name), "\" width=\"", property3.PngProperty.Width, "\" height=\"", property3.PngProperty.Height, "\">") + LineBreak);
                    }
                }

                var newDepth = depth + Indent;
                foreach (var property in property3.WzProperties)
                {
                    {
                        WritePropertyToXml(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</canvas>" + LineBreak);
            }
            else if (prop is WzIntProperty property4)
            {
                tw.Write(string.Concat(depth, "<int name=\"", XmlUtil.SanitizeText(property4.Name), "\" value=\"", property4.Value, "\"/>") + LineBreak);
            }
            else if (prop is WzDoubleProperty property5)
            {
                tw.Write(string.Concat(depth, "<double name=\"", XmlUtil.SanitizeText(property5.Name), "\" value=\"", property5.Value, "\"/>") + LineBreak);
            }
            else if (prop is WzNullProperty property6)
            {
                tw.Write(depth + "<null name=\"" + XmlUtil.SanitizeText(property6.Name) + "\"/>" + LineBreak);
            }
            else if (prop is WzSoundProperty property7)
            {
                if (ExportBase64Data)
                {
                    {
                        tw.Write(string.Concat(new object[] { depth, "<sound name=\"", XmlUtil.SanitizeText(property7.Name), "\" length=\"", property7.Length.ToString(), "\" basehead=\"", Convert.ToBase64String(property7.Header), "\" basedata=\"", Convert.ToBase64String(property7.GetBytes(false)), "\"/>" }) + LineBreak);
                    }
                }
                else
                {
                    {
                        tw.Write(depth + "<sound name=\"" + XmlUtil.SanitizeText(property7.Name) + "\"/>" + LineBreak);
                    }
                }
            }
            else if (prop is WzStringProperty property8)
            {
                var str = XmlUtil.SanitizeText(property8.Value);
                tw.Write(depth + "<string name=\"" + XmlUtil.SanitizeText(property8.Name) + "\" value=\"" + str + "\"/>" + LineBreak);
            }
            else if (prop is WzSubProperty property9)
            {
                tw.Write(depth + "<imgdir name=\"" + XmlUtil.SanitizeText(property9.Name) + "\">" + LineBreak);
                var newDepth = depth + Indent;
                foreach (var property in property9.WzProperties)
                {
                    {
                        WritePropertyToXml(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</imgdir>" + LineBreak);
            }
            else if (prop is WzShortProperty property10)
            {
                tw.Write(string.Concat(depth, "<short name=\"", XmlUtil.SanitizeText(property10.Name), "\" value=\"", property10.Value, "\"/>") + LineBreak);
            }
            else if (prop is WzLongProperty longProp)
            {
                tw.Write(string.Concat(depth, "<long name=\"", XmlUtil.SanitizeText(longProp.Name), "\" value=\"", longProp.Value, "\"/>") + LineBreak);
            }
            else if (prop is WzUOLProperty property11)
            {
                tw.Write(depth + "<uol name=\"" + property11.Name + "\" value=\"" + XmlUtil.SanitizeText(property11.Value) + "\"/>" + LineBreak);
            }
            else if (prop is WzVectorProperty property12)
            {
                tw.Write(string.Concat(depth, "<vector name=\"", XmlUtil.SanitizeText(property12.Name), "\" x=\"", property12.X.Value, "\" y=\"", property12.Y.Value, "\"/>") + LineBreak);
            }
            else if (prop is WzFloatProperty property13)
            {
                var str2 = Convert.ToString(property13.Value, FormattingInfo);
                if (!str2.Contains("."))
                {
                    {
                        str2 = str2 + ".0";
                    }
                }

                tw.Write(depth + "<float name=\"" + XmlUtil.SanitizeText(property13.Name) + "\" value=\"" + str2 + "\"/>" + LineBreak);
            }
            else if (prop is WzConvexProperty property14)
            {
                tw.Write(depth + "<extended name=\"" + XmlUtil.SanitizeText(prop.Name) + "\">" + LineBreak);
                var newDepth = depth + Indent;
                foreach (var property in property14.WzProperties)
                {
                    {
                        WritePropertyToXml(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</extended>" + LineBreak);
            }
        }
    }

    public interface IWzFileSerializer
    {
        void SerializeFile(WzFile file, string path);
    }

    public interface IWzDirectorySerializer : IWzFileSerializer
    {
        void SerializeDirectory(WzDirectory dir, string path);
    }

    public interface IWzImageSerializer : IWzDirectorySerializer
    {
        void SerializeImage(WzImage img, string path);
    }

    public interface IWzObjectSerializer
    {
        void SerializeObject(WzObject file, string path);
    }

    public enum LineBreak
    {
        None,
        Windows,
        Unix
    }

    public class NoBase64DataException : Exception
    {
        public NoBase64DataException() { }
        public NoBase64DataException(string message) : base(message) { }
        public NoBase64DataException(string message, Exception inner) : base(message, inner) { }
        protected NoBase64DataException(SerializationInfo info,
            StreamingContext context)
        { }
    }

    public class WzImgSerializer : ProgressingWzSerializer, IWzImageSerializer
    {
        private byte[] SerializeImageInternal(WzImage img)
        {
            var stream = new MemoryStream();
            var wzWriter = new WzBinaryWriter(stream, ((WzDirectory)img.Parent).WzIv);
            img.SaveImage(wzWriter);
            var result = stream.ToArray();
            wzWriter.Close();
            return result;
        }

        private void SerializeImageInternal(WzImage img, string outPath)
        {
            var stream = File.Create(outPath);
            var wzWriter = new WzBinaryWriter(stream, ((WzDirectory)img.Parent).WzIv);
            img.SaveImage(wzWriter);
            wzWriter.Close();
        }

        public byte[] SerializeImage(WzImage img)
        {
            Total = 1; Current = 0;
            return SerializeImageInternal(img);
        }

        public void SerializeImage(WzImage img, string outPath)
        {
            Total = 1; Current = 0;
            if (Path.GetExtension(outPath) != ".img")
            {
                {
                    outPath += ".img";
                }
            }

            SerializeImageInternal(img, outPath);
        }

        public void SerializeDirectory(WzDirectory dir, string outPath)
        {
            Total = dir.CountImages();
            Current = 0;
            if (!Directory.Exists(outPath))
            {
                {
                    CreateDirSafe(ref outPath);
                }
            }

            if (outPath.Substring(outPath.Length - 1, 1) != @"\")
            {
                {
                    outPath += @"\";
                }
            }

            foreach (var subdir in dir.WzDirectories)
            {
                {
                    SerializeDirectory(subdir, outPath + subdir.Name + @"\");
                }
            }

            foreach (var img in dir.WzImages)
            {
                {
                    SerializeImage(img, outPath + img.Name);
                }
            }
        }

        public void SerializeFile(WzFile f, string outPath)
        {
            SerializeDirectory(f.WzDirectory, outPath);
        }
    }


    public class WzImgDeserializer : ProgressingWzSerializer
    {
        private readonly bool _freeResources;

        public WzImgDeserializer(bool freeResources)
        {
            _freeResources = freeResources;
        }

        public WzImage WzImageFromImgBytes(byte[] bytes, WzMapleVersion version, string name, bool freeResources)
        {
            var iv = WzTool.GetIvByMapleVersion(version);
            var stream = new MemoryStream(bytes);
            var wzReader = new WzBinaryReader(stream, iv);
            var img = new WzImage(name, wzReader)
            {
                BlockSize = bytes.Length,
                Checksum = 0
            };
            foreach (var b in bytes)
            {
                {
                    img.Checksum += b;
                }
            }

            img.Offset = 0;
            if (freeResources)
            {
                img.ParseImage(true);
                img.Changed = true;
                wzReader.Close();
            }
            return img;
        }

        public WzImage WzImageFromImgFile(string inPath, byte[] iv, string name)
        {
            var stream = File.OpenRead(inPath);
            var wzReader = new WzBinaryReader(stream, iv);
            var img = new WzImage(name, wzReader)
            {
                BlockSize = (int)stream.Length,
                Checksum = 0
            };
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            stream.Position = 0;
            foreach (var b in bytes)
            {
                {
                    img.Checksum += b;
                }
            }

            img.Offset = 0;
            if (_freeResources)
            {
                img.ParseImage(true);
                img.Changed = true;
                wzReader.Close();
            }
            return img;
        }
    }


    public class WzPngMp3Serializer : ProgressingWzSerializer, IWzImageSerializer, IWzObjectSerializer
    {
        //List<WzImage> imagesToUnparse = new List<WzImage>();
        private string _outPath;

        public void SerializeObject(WzObject obj, string outPath)
        {
            //imagesToUnparse.Clear();
            Total = 0; Current = 0;
            _outPath = outPath;
            if (!Directory.Exists(outPath))
            {
                {
                    CreateDirSafe(ref outPath);
                }
            }

            if (outPath.Substring(outPath.Length - 1, 1) != @"\")
            {
                {
                    outPath += @"\";
                }
            }

            Total = CalculateTotal(obj);
            Export(obj, outPath);
            /*foreach (WzImage img in imagesToUnparse)
                img.UnparseImage();
            imagesToUnparse.Clear();*/
        }

        public void SerializeFile(WzFile file, string path)
        {
            SerializeObject(file, path);
        }

        public void SerializeDirectory(WzDirectory file, string path)
        {
            SerializeObject(file, path);
        }

        public void SerializeImage(WzImage file, string path)
        {
            SerializeObject(file, path);
        }

        private int CalculateTotal(WzObject currObj)
        {
            var result = 0;
            if (currObj is WzFile)
            {
                {
                    result += ((WzFile)currObj).WzDirectory.CountImages();
                }
            }
            else if (currObj is WzDirectory)
            {
                {
                    result += ((WzDirectory)currObj).CountImages();
                }
            }

            return result;
        }

        private void Export(WzObject currObj, string exportOutPath)
        {
            while (true)
            {
                if (currObj is WzFile)
                {
                    {
                        currObj = ((WzFile) currObj).WzDirectory;
                        continue;
                    }
                }

                if (currObj is WzDirectory directory)
                {
                    exportOutPath += directory.Name + @"\";
                    if (!Directory.Exists(exportOutPath))
                    {
                        {
                            Directory.CreateDirectory(exportOutPath);
                        }
                    }

                    foreach (var subdir in directory.WzDirectories)
                    {
                        {
                            Export(subdir, exportOutPath + subdir.Name + @"\");
                        }
                    }

                    foreach (var subimg in directory.WzImages)
                    {
                        {
                            Export(subimg, exportOutPath + subimg.Name + @"\");
                        }
                    }
                }
                else if (currObj is WzCanvasProperty property)
                {
                    var bmp = property.PngProperty.GetPNG(false);
                    var path = exportOutPath + property.Name + ".png";
                    bmp.Save(path, ImageFormat.Png);
                    //Current++;
                }
                else if (currObj is WzSoundProperty soundProperty)
                {
                    var path = exportOutPath + soundProperty.Name + ".mp3";
                    soundProperty.SaveToFile(path);
                }
                else if (currObj is WzImage image)
                {
                    exportOutPath += image.Name + @"\";
                    if (!Directory.Exists(exportOutPath))
                    {
                        {
                            Directory.CreateDirectory(exportOutPath);
                        }
                    }

                    var parse = image.Parsed || image.Changed;
                    if (!parse)
                    {
                        {
                            image.ParseImage();
                        }
                    }

                    foreach (var subprop in image.WzProperties)
                    {
                        {
                            Export(subprop, exportOutPath);
                        }
                    }

                    if (!parse)
                    {
                        {
                            image.UnparseImage();
                        }
                    }

                    Current++;
                }
                else if (currObj is IPropertyContainer container)
                {
                    exportOutPath += currObj.Name + ".";
                    foreach (var subprop in container.WzProperties)
                    {
                        {
                            Export(subprop, exportOutPath);
                        }
                    }
                }
                else if (currObj is WzUOLProperty)
                {
                    {
                        currObj = ((WzUOLProperty) currObj).LinkValue;
                        continue;
                    }
                }

                break;
            }
        }
    }

    public class WzClassicXmlSerializer : WzXmlSerializer, IWzImageSerializer
    {
        public WzClassicXmlSerializer(int indentation, LineBreak lineBreakType, bool exportbase64)
            : base(indentation, lineBreakType)
        { ExportBase64Data = exportbase64; }

        private void ExportXmlInternal(WzImage img, string path)
        {
            var parsed = img.Parsed || img.Changed;
            if (!parsed)
            {
                {
                    img.ParseImage();
                }
            }

            Current++;
            TextWriter tw = new StreamWriter(path);
            tw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" + LineBreak);
            tw.Write("<imgdir name=\"" + XmlUtil.SanitizeText(img.Name) + "\">" + LineBreak);
            foreach (var property in img.WzProperties)
            {
                {
                    WritePropertyToXml(tw, Indent, property);
                }
            }

            tw.Write("</imgdir>" + LineBreak);
            tw.Close();
            if (!parsed)
            {
                {
                    img.UnparseImage();
                }
            }
        }

        private void ExportDirXmlInternal(WzDirectory dir, string path)
        {
            if (!Directory.Exists(path))
            {
                {
                    CreateDirSafe(ref path);
                }
            }

            if (path.Substring(path.Length - 1) != @"\")
            {
                {
                    path += @"\";
                }
            }

            foreach (var subdir in dir.WzDirectories)
            {
                {
                    ExportDirXmlInternal(subdir, path + subdir.Name + @"\");
                }
            }

            foreach (var subimg in dir.WzImages)
            {
                {
                    ExportXmlInternal(subimg, path + subimg.Name + ".xml");
                }
            }
        }

        public void SerializeImage(WzImage img, string path)
        {
            Total = 1; Current = 0;
            if (Path.GetExtension(path) != ".xml")
            {
                {
                    path += ".xml";
                }
            }

            ExportXmlInternal(img, path);
        }

        public void SerializeDirectory(WzDirectory dir, string path)
        {
            Total = dir.CountImages(); Current = 0;
            ExportDirXmlInternal(dir, path);
        }

        public void SerializeFile(WzFile file, string path)
        {
            SerializeDirectory(file.WzDirectory, path);
        }
    }

    public class WzNewXmlSerializer : WzXmlSerializer
    {
        public WzNewXmlSerializer(int indentation, LineBreak lineBreakType)
            : base(indentation, lineBreakType)
        { }

        internal void DumpImageToXml(TextWriter tw, string depth, WzImage img)
        {
            var parsed = img.Parsed || img.Changed;
            if (!parsed)
            {
                {
                    img.ParseImage();
                }
            }

            Current++;
            tw.Write(depth + "<wzimg name=\"" + XmlUtil.SanitizeText(img.Name) + "\">" + LineBreak);
            var newDepth = depth + Indent;
            foreach (var property in img.WzProperties)
            {
                {
                    WritePropertyToXml(tw, newDepth, property);
                }
            }

            tw.Write(depth + "</wzimg>");
            if (!parsed)
            {
                {
                    img.UnparseImage();
                }
            }
        }

        internal void DumpDirectoryToXml(TextWriter tw, string depth, WzDirectory dir)
        {
            tw.Write(depth + "<wzdir name=\"" + XmlUtil.SanitizeText(dir.Name) + "\">" + LineBreak);
            foreach (var subdir in dir.WzDirectories)
            {
                {
                    DumpDirectoryToXml(tw, depth + Indent, subdir);
                }
            }

            foreach (var img in dir.WzImages)
            {
                {
                    DumpImageToXml(tw, depth + Indent, img);
                }
            }

            tw.Write(depth + "</wzdir>" + LineBreak);
        }

        public void ExportCombinedXml(List<WzObject> objects, string path)
        {
            Total = 1; Current = 0;
            if (Path.GetExtension(path) != ".xml")
            {
                {
                    path += ".xml";
                }
            }

            foreach (var obj in objects)
            {
                if (obj is WzImage)
                {
                    {
                        Total++;
                    }
                }
                else if (obj is WzDirectory directory)
                {
                    {
                        Total += directory.CountImages();
                    }
                }
            }

            ExportBase64Data = true;
            TextWriter tw = new StreamWriter(path);
            tw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" + LineBreak);
            tw.Write("<xmldump>" + LineBreak);
            foreach (var obj in objects)
            {
                if (obj is WzDirectory directory)
                {
                    {
                        DumpDirectoryToXml(tw, Indent, directory);
                    }
                }
                else if (obj is WzImage image)
                {
                    {
                        DumpImageToXml(tw, Indent, image);
                    }
                }
                else if (obj is WzImageProperty property)
                {
                    {
                        WritePropertyToXml(tw, Indent, property);
                    }
                }
            }
            tw.Write("</xmldump>" + LineBreak);
            tw.Close();
        }
    }

    public class WzXmlDeserializer : ProgressingWzSerializer
    {
        public static NumberFormatInfo FormattingInfo;

        private readonly bool _useMemorySaving;
        private readonly byte[] _iv;
        private readonly WzImgDeserializer _imgDeserializer = new WzImgDeserializer(false);

        public WzXmlDeserializer(bool useMemorySaving, byte[] iv)
        {
            _useMemorySaving = useMemorySaving;
            _iv = iv;
        }

        #region Public Functions
        public List<WzObject> ParseXml(string path)
        {
            var result = new List<WzObject>();
            var doc = new XmlDocument();
            doc.Load(path);
            var mainElement = (XmlElement)doc.ChildNodes[1];
            Current = 0;
            if (mainElement.Name == "xmldump")
            {
                Total = CountImgs(mainElement);
                foreach (XmlElement subelement in mainElement)
                {
                    if (subelement.Name == "wzdir")
                    {
                        {
                            result.Add(ParseXmlWzDir(subelement));
                        }
                    }
                    else if (subelement.Name == "wzimg")
                    {
                        {
                            result.Add(ParseXmlWzImg(subelement));
                        }
                    }
                    else
                    {
                        {
                            throw new InvalidDataException("unknown XML prop " + subelement.Name);
                        }
                    }
                }
            }
            else if (mainElement.Name == "imgdir")
            {
                Total = 1;
                result.Add(ParseXmlWzImg(mainElement));
                Current++;
            }
            else
            {
                {
                    throw new InvalidDataException("unknown main XML prop " + mainElement.Name);
                }
            }

            return result;
        }
        #endregion

        #region Internal Functions
        internal int CountImgs(XmlElement element)
        {
            var result = 0;
            foreach (XmlElement subelement in element)
            {
                if (subelement.Name == "wzimg")
                {
                    {
                        result++;
                    }
                }
                else if (subelement.Name == "wzdir")
                {
                    {
                        result += CountImgs(subelement);
                    }
                }
            }
            return result;
        }

        internal WzDirectory ParseXmlWzDir(XmlElement dirElement)
        {
            var result = new WzDirectory(dirElement.GetAttribute("name"));
            foreach (XmlElement subelement in dirElement)
            {
                if (subelement.Name == "wzdir")
                {
                    {
                        result.AddDirectory(ParseXmlWzDir(subelement));
                    }
                }
                else if (subelement.Name == "wzimg")
                {
                    {
                        result.AddImage(ParseXmlWzImg(subelement));
                    }
                }
                else
                {
                    {
                        throw new InvalidDataException("unknown XML prop " + subelement.Name);
                    }
                }
            }
            return result;
        }

        internal WzImage ParseXmlWzImg(XmlElement imgElement)
        {
            var name = imgElement.GetAttribute("name");
            var result = new WzImage(name);
            foreach (XmlElement subelement in imgElement)
            {
                {
                    result.WzProperties.Add(ParsePropertyFromXmlElement(subelement));
                }
            }

            result.Changed = true;
            if (_useMemorySaving)
            {
                var path = Path.GetTempFileName();
                var wzWriter = new WzBinaryWriter(File.Create(path), _iv);
                result.SaveImage(wzWriter);
                wzWriter.Close();
                result.Dispose();
                result = _imgDeserializer.WzImageFromImgFile(path, _iv, name);
            }
            return result;
        }

        internal WzImageProperty ParsePropertyFromXmlElement(XmlElement element)
        {
            switch (element.Name)
            {
                case "imgdir":
                    var sub = new WzSubProperty(element.GetAttribute("name"));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            sub.AddProperty(ParsePropertyFromXmlElement(subelement));
                        }
                    }

                    return sub;

                case "canvas":
                    var canvas = new WzCanvasProperty(element.GetAttribute("name"));
                    if (!element.HasAttribute("basedata"))
                    {
                        {
                            throw new NoBase64DataException("no base64 data in canvas element with name " + canvas.Name);
                        }
                    }

                    canvas.PngProperty = new WzPngProperty();
                    var pngstream = new MemoryStream(Convert.FromBase64String(element.GetAttribute("basedata")));
                    canvas.PngProperty.SetPNG((Bitmap)Image.FromStream(pngstream));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            canvas.AddProperty(ParsePropertyFromXmlElement(subelement));
                        }
                    }

                    return canvas;

                case "int":
                    var compressedInt = new WzIntProperty(element.GetAttribute("name"), int.Parse(element.GetAttribute("value"), FormattingInfo));
                    return compressedInt;

                case "double":
                    var doubleProp = new WzDoubleProperty(element.GetAttribute("name"), double.Parse(element.GetAttribute("value"), FormattingInfo));
                    return doubleProp;

                case "null":
                    var nullProp = new WzNullProperty(element.GetAttribute("name"));
                    return nullProp;

                case "sound":
                    if (!element.HasAttribute("basedata") || !element.HasAttribute("basehead") || !element.HasAttribute("length"))
                    {
                        {
                            throw new NoBase64DataException("no base64 data in sound element with name " + element.GetAttribute("name"));
                        }
                    }

                    var sound = new WzSoundProperty(element.GetAttribute("name"),
                        int.Parse(element.GetAttribute("length")),
                        Convert.FromBase64String(element.GetAttribute("basehead")),
                        Convert.FromBase64String(element.GetAttribute("basedata")));
                    return sound;

                case "string":
                    var stringProp = new WzStringProperty(element.GetAttribute("name"), element.GetAttribute("value"));
                    return stringProp;

                case "short":
                    var shortProp = new WzShortProperty(element.GetAttribute("name"), short.Parse(element.GetAttribute("value"), FormattingInfo));
                    return shortProp;

                case "long":
                    var longProp = new WzLongProperty(element.GetAttribute("name"), long.Parse(element.GetAttribute("value"), FormattingInfo));
                    return longProp;

                case "uol":
                    var uol = new WzUOLProperty(element.GetAttribute("name"), element.GetAttribute("value"));
                    return uol;

                case "vector":
                    var vector = new WzVectorProperty(element.GetAttribute("name"), new WzIntProperty("x", Convert.ToInt32(element.GetAttribute("x"))), new WzIntProperty("y", Convert.ToInt32(element.GetAttribute("y"))));
                    return vector;

                case "float":
                    var floatProp = new WzFloatProperty(element.GetAttribute("name"), float.Parse(element.GetAttribute("value"), FormattingInfo));
                    return floatProp;

                case "extended":
                    var convex = new WzConvexProperty(element.GetAttribute("name"));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            convex.AddProperty(ParsePropertyFromXmlElement(subelement));
                        }
                    }

                    return convex;
            }
            throw new InvalidDataException("unknown XML prop " + element.Name);
        }
        #endregion
    }
}