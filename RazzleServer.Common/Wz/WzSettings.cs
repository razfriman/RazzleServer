﻿using System;
using System.DrawingCore;
using System.IO;
using System.Reflection;
using RazzleServer.Common.Wz.WzProperties;
using RazzleServer.Common.Wz.WzStructure;

namespace RazzleServer.Common.Wz
{
    public class WzSettingsManager
    {
        private readonly string wzPath;
        private readonly Type userSettingsType;
        private readonly Type appSettingsType;
        private readonly Type xnaColorType;


        public WzSettingsManager(string wzPath, Type userSettingsType, Type appSettingsType)
        {
            this.wzPath = wzPath;
            this.userSettingsType = userSettingsType;
            this.appSettingsType = appSettingsType;
        }

        public WzSettingsManager(string wzPath, Type userSettingsType, Type appSettingsType, Type xnaColorType)
            : this(wzPath, userSettingsType, appSettingsType)
        {
            this.xnaColorType = xnaColorType;
        }

        private void ExtractSettingsImage(WzImage settingsImage, Type settingsHolderType)
        {
            if (!settingsImage.Parsed)
            {
                settingsImage.ParseImage();
            }

            foreach (var fieldInfo in settingsHolderType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var settingName = fieldInfo.Name;
                var settingProp = settingsImage[settingName];
                byte[] argb;
                if (settingProp == null)
                {
                    SaveField(settingsImage, fieldInfo);
                }
                else if (fieldInfo.FieldType.BaseType != null && fieldInfo.FieldType.BaseType.FullName == "System.Enum")
                {
                    fieldInfo.SetValue(null, InfoTool.GetInt(settingProp));
                }
                else
                {
                    switch (fieldInfo.FieldType.FullName)
                    {
                        case "System.DrawingCore.Color":
                            argb = BitConverter.GetBytes((uint)((WzDoubleProperty)settingProp).Value);
                            fieldInfo.SetValue(null, Color.FromArgb(argb[3], argb[2], argb[1], argb[0]));
                            break;
                        case "System.Int32":
                            fieldInfo.SetValue(null, InfoTool.GetInt(settingProp));
                            break;
                        case "System.Double":
                            fieldInfo.SetValue(null, ((WzDoubleProperty)settingProp).Value);
                            break;
                        case "System.Boolean":
                            fieldInfo.SetValue(null, InfoTool.GetBool(settingProp));
                            //bool a = (bool)fieldInfo.GetValue(null);
                            break;
                        case "System.Single":
                            fieldInfo.SetValue(null, ((WzFloatProperty)settingProp).Value);
                            break;
                        case "System.String":
                            fieldInfo.SetValue(null, InfoTool.GetString(settingProp));
                            break;
                        case "System.DrawingCore.Imaging.Bitmap":
                            fieldInfo.SetValue(null, ((WzCanvasProperty)settingProp).PngProperty.GetPNG(false));
                            break;
                        default:
                            throw new Exception("unrecognized setting type");
                    }
                }
            }
        }

        private void CreateWzProp(IPropertyContainer parent, WzPropertyType propType, string propName, object value)
        {
            WzImageProperty addedProp;
            switch (propType)
            {
                case WzPropertyType.Float:
                    addedProp = new WzFloatProperty(propName);
                    break;
                case WzPropertyType.Canvas:
                    addedProp = new WzCanvasProperty(propName);
                    ((WzCanvasProperty)addedProp).PngProperty = new WzPngProperty();
                    break;
                case WzPropertyType.Int:
                    addedProp = new WzIntProperty(propName);
                    break;
                case WzPropertyType.Double:
                    addedProp = new WzDoubleProperty(propName);
                    break;
                /*case WzPropertyType.Sound:
                    addedProp = new WzSoundProperty(propName);
                    break;*/
                case WzPropertyType.String:
                    addedProp = new WzStringProperty(propName);
                    break;
                case WzPropertyType.Short:
                    addedProp = new WzShortProperty(propName);
                    break;
                case WzPropertyType.Vector:
                    addedProp = new WzVectorProperty(propName);
                    ((WzVectorProperty)addedProp).X = new WzIntProperty("X");
                    ((WzVectorProperty)addedProp).Y = new WzIntProperty("Y");
                    break;
                default:
                    throw new NotSupportedException("not supported type");
            }
            addedProp.SetValue(value);
            parent.AddProperty(addedProp);
        }

        private void SetWzProperty(WzImage parentImage, string propName, WzPropertyType propType, object value)
        {
            var property = parentImage[propName];
            if (property != null)
            {
                if (property.PropertyType == propType)
                {
                    property.SetValue(value);
                }
                else
                {
                    property.Remove();
                    CreateWzProp(parentImage, propType, propName, value);
                }
            }
            else
            {
                CreateWzProp(parentImage, propType, propName, value);
            }
        }

        private void SaveSettingsImage(WzImage settingsImage, Type settingsHolderType)
        {
            if (!settingsImage.Parsed)
            {
                settingsImage.ParseImage();
            }

            foreach (var fieldInfo in settingsHolderType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                SaveField(settingsImage, fieldInfo);
            }
            settingsImage.Changed = true;
        }

        private void SaveField(WzImage settingsImage, FieldInfo fieldInfo)
        {
            var settingName = fieldInfo.Name;
            if (fieldInfo.FieldType.BaseType != null && fieldInfo.FieldType.BaseType.FullName == "System.Enum")
            {
                SetWzProperty(settingsImage, settingName, WzPropertyType.Int, fieldInfo.GetValue(null));
            }
            else
            {
                switch (fieldInfo.FieldType.FullName)
                {
                    case "System.DrawingCore.Color":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Double, (double)((Color)fieldInfo.GetValue(null)).ToArgb());
                        break;
                    case "System.Int32":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Int, fieldInfo.GetValue(null));
                        break;
                    case "System.Double":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Double, fieldInfo.GetValue(null));
                        break;
                    case "Single":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Float, fieldInfo.GetValue(null));
                        break;
                    case "System.String":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.String, fieldInfo.GetValue(null));
                        break;
                    case "System.DrawingCore.Imaging.Bitmap":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Canvas, fieldInfo.GetValue(null));
                        break;
                    case "System.Boolean":
                        SetWzProperty(settingsImage, settingName, WzPropertyType.Int, (bool)fieldInfo.GetValue(null) ? 1 : 0);
                        break;
                }
            }
        }

        public void Load()
        {
            if (!File.Exists(wzPath))
            {
                return;
            }
         
            using (var wzFile = new WzFile(wzPath, 1337, WzMapleVersion.Classic))
            {
                wzFile.ParseWzFile();
                ExtractSettingsImage((WzImage)wzFile["UserSettings.img"], userSettingsType);
                ExtractSettingsImage((WzImage)wzFile["ApplicationSettings.img"], appSettingsType);
            }
        }

        public void Save()
        {
            var settingsExist = File.Exists(wzPath);
            WzFile wzFile;
            if (settingsExist)
            {
                wzFile = new WzFile(wzPath, 1337, WzMapleVersion.Classic);
                wzFile.ParseWzFile();
            }
            else
            {
                wzFile = new WzFile(1337, WzMapleVersion.Classic) {Header = {Copyright = "MapleLib"}};
                wzFile.Header.RecalculateFileStart();
                var US = new WzImage("UserSettings.img") { Changed = true, Parsed = true };
                var AS = new WzImage("ApplicationSettings.img") { Changed = true, Parsed = true };
                wzFile.WzDirectory.WzImages.Add(US);
                wzFile.WzDirectory.WzImages.Add(AS);
            }
            SaveSettingsImage((WzImage)wzFile["UserSettings.img"], userSettingsType);
            SaveSettingsImage((WzImage)wzFile["ApplicationSettings.img"], appSettingsType);
            if (settingsExist)
            {
                var tempFile = Path.GetTempFileName();
                var settingsPath = wzFile.FilePath;
                wzFile.SaveToDisk(tempFile);
                wzFile.Dispose();
                File.Delete(settingsPath);
                File.Move(tempFile, settingsPath);
            }
            else
            {
                wzFile.SaveToDisk(wzPath);
            }
        }
    }
}