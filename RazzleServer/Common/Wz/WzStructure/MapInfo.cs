using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.WzProperties;
using RazzleServer.Common.Wz.WzStructure.Data;

namespace RazzleServer.Common.Wz.WzStructure
{
    public class MapInfo //Credits to Bui for some of the info
    {
        public static ILogger Log = LogManager.Log;

        public static MapInfo Default = new MapInfo();

        public MapInfo()
        {
        }

        public MapInfo(WzImage image, string strMapName, string strStreetName, string strCategoryName)
        {
            Image = image;
            int? startHour;
            int? endHour;
            this.strMapName = strMapName;
            this.strStreetName = strStreetName;
            this.strCategoryName = strCategoryName;
            var file = image.WzFileParent;
            var loggerSuffix = ", map " + image.Name + (file != null ? " of version " + Enum.GetName(typeof(WzMapleVersion), file.MapleVersion) + ", v" + file.Version : "");
            foreach (var prop in image["info"].WzProperties)
            {
                switch (prop.Name)
                {
                    case "bgm":
                        bgm = InfoTool.GetString(prop);
                        break;
                    case "cloud":
                        cloud = InfoTool.GetBool(prop);
                        break;
                    case "swim":
                        swim = InfoTool.GetBool(prop);
                        break;
                    case "forcedReturn":
                        forcedReturn = InfoTool.GetInt(prop);
                        break;
                    case "hideMinimap":
                        hideMinimap = InfoTool.GetBool(prop);
                        break;
                    case "mapDesc":
                        mapDesc = InfoTool.GetString(prop);
                        break;
                    case "mapName":
                        mapName = InfoTool.GetString(prop);
                        break;
                    case "mapMark":
                        mapMark = InfoTool.GetString(prop);
                        break;
                    case "mobRate":
                        mobRate = InfoTool.GetFloat(prop);
                        break;
                    case "moveLimit":
                        moveLimit = InfoTool.GetInt(prop);
                        break;
                    case "returnMap":
                        returnMap = InfoTool.GetInt(prop);
                        break;
                    case "town":
                        town = InfoTool.GetBool(prop);
                        break;
                    case "version":
                        version = InfoTool.GetInt(prop);
                        break;
                    case "fieldLimit":
                        var fl = InfoTool.GetInt(prop);
                        if (fl >= (int)Math.Pow(2, 23))
                        {
                            Log.LogError($"Invalid fieldlimit: {fl}");
                            fl = fl & ((int)Math.Pow(2, 23) - 1);
                        }
                        fieldLimit = (FieldLimit)fl;
                        break;
                    case "VRTop":
                    case "VRBottom":
                    case "VRLeft":
                    case "VRRight":
                        break;
                    case "link":
                        //link = InfoTool.GetInt(prop);
                        break;
                    case "timeLimit":
                        timeLimit = InfoTool.GetInt(prop);
                        break;
                    case "lvLimit":
                        lvLimit = InfoTool.GetInt(prop);
                        break;
                    case "onFirstUserEnter":
                        onFirstUserEnter = InfoTool.GetString(prop);
                        break;
                    case "onUserEnter":
                        onUserEnter = InfoTool.GetString(prop);
                        break;
                    case "fly":
                        fly = InfoTool.GetBool(prop);
                        break;
                    case "noMapCmd":
                        noMapCmd = InfoTool.GetBool(prop);
                        break;
                    case "partyOnly":
                        partyOnly = InfoTool.GetBool(prop);
                        break;
                    case "fieldType":
                        var ft = InfoTool.GetInt(prop);
                        if (!Enum.IsDefined(typeof(FieldType), ft))
                        {
                            Log.LogError($"Invalid fieldType: {ft}");
                            ft = 0;
                        }
                        fieldType = (FieldType)ft;
                        break;
                    case "miniMapOnOff":
                        miniMapOnOff = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffle":
                        reactorShuffle = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffleName":
                        reactorShuffleName = InfoTool.GetString(prop);
                        break;
                    case "personalShop":
                        personalShop = InfoTool.GetBool(prop);
                        break;
                    case "entrustedShop":
                        entrustedShop = InfoTool.GetBool(prop);
                        break;
                    case "effect":
                        effect = InfoTool.GetString(prop);
                        break;
                    case "lvForceMove":
                        lvForceMove = InfoTool.GetInt(prop);
                        break;
                    case "timeMob":
                        startHour = InfoTool.GetOptionalInt(prop["startHour"]);
                        endHour = InfoTool.GetOptionalInt(prop["endHour"]);
                        var propId = InfoTool.GetOptionalInt(prop["id"]);
                        var message = InfoTool.GetOptionalString(prop["message"]);
                        if (propId == null || message == null || startHour == null ^ endHour == null)
                        {
                            Log.LogError("timeMob is missing data");
                        }
                        else
                        {
                            timeMob = new TimeMob(startHour, endHour, (int)propId, message);
                        }

                        break;
                    case "help":
                        help = InfoTool.GetString(prop);
                        break;
                    case "snow":
                        snow = InfoTool.GetBool(prop);
                        break;
                    case "rain":
                        rain = InfoTool.GetBool(prop);
                        break;
                    case "dropExpire":
                        dropExpire = InfoTool.GetInt(prop);
                        break;
                    case "decHP":
                        decHP = InfoTool.GetInt(prop);
                        break;
                    case "decInterval":
                        decInterval = InfoTool.GetInt(prop);
                        break;
                    case "expeditionOnly":
                        expeditionOnly = InfoTool.GetBool(prop);
                        break;
                    case "fs":
                        fs = InfoTool.GetFloat(prop);
                        break;
                    case "protectItem":
                        protectItem = InfoTool.GetInt(prop);
                        break;
                    case "createMobInterval":
                        createMobInterval = InfoTool.GetInt(prop);
                        break;
                    case "fixedMobCapacity":
                        fixedMobCapacity = InfoTool.GetInt(prop);
                        break;
                    case "streetName":
                        streetName = InfoTool.GetString(prop);
                        break;
                    case "noRegenMap":
                        noRegenMap = InfoTool.GetBool(prop);
                        break;
                    case "allowedItem":
                        allowedItem = new List<int>();
                        if (prop.WzProperties != null && prop.WzProperties.Count > 0)
                        {
                            foreach (var item in prop.WzProperties)
                            {
                                allowedItem.Add(item.GetInt());
                            }
                        }

                        break;
                    case "recovery":
                        recovery = InfoTool.GetFloat(prop);
                        break;
                    case "blockPBossChange":
                        blockPBossChange = InfoTool.GetBool(prop);
                        break;
                    case "everlast":
                        everlast = InfoTool.GetBool(prop);
                        break;
                    case "damageCheckFree":
                        damageCheckFree = InfoTool.GetBool(prop);
                        break;
                    case "dropRate":
                        dropRate = InfoTool.GetFloat(prop);
                        break;
                    case "scrollDisable":
                        scrollDisable = InfoTool.GetBool(prop);
                        break;
                    case "needSkillForFly":
                        needSkillForFly = InfoTool.GetBool(prop);
                        break;
                    case "zakum2Hack":
                        zakum2Hack = InfoTool.GetBool(prop);
                        break;
                    case "allMoveCheck":
                        allMoveCheck = InfoTool.GetBool(prop);
                        break;
                    case "VRLimit":
                        VRLimit = InfoTool.GetBool(prop);
                        break;
                    case "consumeItemCoolTime":
                        consumeItemCoolTime = InfoTool.GetBool(prop);
                        break;
                    default:
                        Log.LogWarning($"Unknown property: {prop.Name}");
                        additionalProps.Add(prop.DeepClone());
                        break;
                }
            }
        }

        public static Rectangle GetVR(WzImage image)
        {
            Rectangle result = null;
            if (image["info"]["VRLeft"] != null)
            {
                var info = image["info"];
                var left = InfoTool.GetInt(info["VRLeft"]);
                var right = InfoTool.GetInt(info["VRRight"]);
                var top = InfoTool.GetInt(info["VRTop"]);
                var bottom = InfoTool.GetInt(info["VRBottom"]);
                result = new Rectangle(left, top, right, bottom);
            }
            return result;
        }

        public void Save(WzImage dest, Rectangle VR)
        {
            var info = new WzSubProperty
            {
                ["bgm"] = InfoTool.SetString(bgm),
                ["cloud"] = InfoTool.SetBool(cloud),
                ["swim"] = InfoTool.SetBool(swim),
                ["forcedReturn"] = InfoTool.SetInt(forcedReturn),
                ["hideMinimap"] = InfoTool.SetBool(hideMinimap),
                ["mapDesc"] = InfoTool.SetOptionalString(mapDesc),
                ["mapName"] = InfoTool.SetOptionalString(mapDesc),
                ["mapMark"] = InfoTool.SetString(mapMark),
                ["mobRate"] = InfoTool.SetFloat(mobRate),
                ["moveLimit"] = InfoTool.SetOptionalInt(moveLimit),
                ["returnMap"] = InfoTool.SetInt(returnMap),
                ["town"] = InfoTool.SetBool(town),
                ["version"] = InfoTool.SetInt(version),
                ["fieldLimit"] = InfoTool.SetInt((int) fieldLimit),
                ["timeLimit"] = InfoTool.SetOptionalInt(timeLimit),
                ["lvLimit"] = InfoTool.SetOptionalInt(lvLimit),
                ["onFirstUserEnter"] = InfoTool.SetOptionalString(onFirstUserEnter),
                ["onUserEnter"] = InfoTool.SetOptionalString(onUserEnter),
                ["fly"] = InfoTool.SetOptionalBool(fly),
                ["noMapCmd"] = InfoTool.SetOptionalBool(noMapCmd),
                ["partyOnly"] = InfoTool.SetOptionalBool(partyOnly),
                ["fieldType"] = InfoTool.SetOptionalInt((int?) fieldType),
                ["miniMapOnOff"] = InfoTool.SetOptionalBool(miniMapOnOff),
                ["reactorShuffle"] = InfoTool.SetOptionalBool(reactorShuffle),
                ["reactorShuffleName"] = InfoTool.SetOptionalString(reactorShuffleName),
                ["personalShop"] = InfoTool.SetOptionalBool(personalShop),
                ["entrustedShop"] = InfoTool.SetOptionalBool(entrustedShop),
                ["effect"] = InfoTool.SetOptionalString(effect),
                ["lvForceMove"] = InfoTool.SetOptionalInt(lvForceMove)
            };
            if (timeMob != null)
            {
                var prop = new WzSubProperty
                {
                    ["startHour"] = InfoTool.SetOptionalInt(timeMob.Value.startHour),
                    ["endHour"] = InfoTool.SetOptionalInt(timeMob.Value.endHour),
                    ["id"] = InfoTool.SetOptionalInt(timeMob.Value.id),
                    ["message"] = InfoTool.SetOptionalString(timeMob.Value.message)
                };
                info["timeMob"] = prop;
            }
            info["help"] = InfoTool.SetOptionalString(help);
            info["snow"] = InfoTool.SetOptionalBool(snow);
            info["rain"] = InfoTool.SetOptionalBool(rain);
            info["dropExpire"] = InfoTool.SetOptionalInt(dropExpire);
            info["decHP"] = InfoTool.SetOptionalInt(decHP);
            info["decInterval"] = InfoTool.SetOptionalInt(decInterval);
            info["expeditionOnly"] = InfoTool.SetOptionalBool(expeditionOnly);
            info["fs"] = InfoTool.SetOptionalFloat(fs);
            info["protectItem"] = InfoTool.SetOptionalInt(protectItem);
            info["createMobInterval"] = InfoTool.SetOptionalInt(createMobInterval);
            info["fixedMobCapacity"] = InfoTool.SetOptionalInt(fixedMobCapacity);
            info["streetName"] = InfoTool.SetOptionalString(streetName);
            info["noRegenMap"] = InfoTool.SetOptionalBool(noRegenMap);
            if (allowedItem != null)
            {
                var prop = new WzSubProperty();
                for (var i = 0; i < allowedItem.Count; i++)
                {
                    prop[i.ToString()] = InfoTool.SetInt(allowedItem[i]);
                }
                info["allowedItem"] = prop;
            }
            info["recovery"] = InfoTool.SetOptionalFloat(recovery);
            info["blockPBossChange"] = InfoTool.SetOptionalBool(blockPBossChange);
            info["everlast"] = InfoTool.SetOptionalBool(everlast);
            info["damageCheckFree"] = InfoTool.SetOptionalBool(damageCheckFree);
            info["dropRate"] = InfoTool.SetOptionalFloat(dropRate);
            info["scrollDisable"] = InfoTool.SetOptionalBool(scrollDisable);
            info["needSkillForFly"] = InfoTool.SetOptionalBool(needSkillForFly);
            info["zakum2Hack"] = InfoTool.SetOptionalBool(zakum2Hack);
            info["allMoveCheck"] = InfoTool.SetOptionalBool(allMoveCheck);
            info["VRLimit"] = InfoTool.SetOptionalBool(VRLimit);
            info["consumeItemCoolTime"] = InfoTool.SetOptionalBool(consumeItemCoolTime);
            foreach (var prop in additionalProps)
            {
                info.AddProperty(prop);
            }
            if (VR != null)
            {
                info["VRLeft"] = InfoTool.SetInt(VR.Lt.X);
                info["VRRight"] = InfoTool.SetInt(VR.Rb.X);
                info["VRTop"] = InfoTool.SetInt(VR.Lt.Y);
                info["VRBottom"] = InfoTool.SetInt(VR.Rb.Y);
            }
            dest["info"] = info;
        }

        //Cannot change
        public int version = 10;

        //Must have
        public string bgm = "Bgm00/GoPicnic";
        public string mapMark = "None";
        public FieldLimit fieldLimit = FieldLimit.FIELDOPT_NONE;
        public int returnMap = 999999999;
        public int forcedReturn = 999999999;
        public bool cloud;
        public bool swim;
        public bool hideMinimap;
        public bool town;
        public float mobRate = 1.5f;

        //Optional
        //public int link = -1;
        public int? timeLimit;
        public int? lvLimit;
        public FieldType? fieldType;
        public string onFirstUserEnter;
        public string onUserEnter;
        public MapleBool fly = null;
        public MapleBool noMapCmd = null;
        public MapleBool partyOnly = null;
        public MapleBool reactorShuffle = null;
        public string reactorShuffleName;
        public MapleBool personalShop = null;
        public MapleBool entrustedShop = null;
        public string effect; //Bubbling; 610030550 and many others
        public int? lvForceMove; //limit FROM value
        public TimeMob? timeMob;
        public string help; //help string
        public MapleBool snow = null;
        public MapleBool rain = null;
        public int? dropExpire; //in seconds
        public int? decHP;
        public int? decInterval;
        public MapleBool expeditionOnly = null;
        public float? fs; //slip on ice speed, default 0.2
        public int? protectItem; //Id, item protecting from cold
        public int? createMobInterval; //used for massacre pqs
        public int? fixedMobCapacity; //mob capacity to target (used for massacre pqs)

        //Unknown optional
        public int? moveLimit;
        public string mapDesc;
        public string mapName;
        public string streetName;
        public MapleBool miniMapOnOff = null;
        public MapleBool noRegenMap = null; //610030400
        public List<int> allowedItem;
        public float? recovery; //recovery rate, like in sauna (3)
        public MapleBool blockPBossChange = null; //something with monster carnival
        public MapleBool everlast = null; //something with bonus stages of PQs
        public MapleBool damageCheckFree = null; //something with fishing event
        public float? dropRate;
        public MapleBool scrollDisable = null;
        public MapleBool needSkillForFly = null;
        public MapleBool zakum2Hack = null; //JQ hack protection
        public MapleBool allMoveCheck = null; //another JQ hack protection
        public MapleBool VRLimit = null; //use vr's as limits?
        public MapleBool consumeItemCoolTime = null; //cool time of consume item

        //Special
        public List<WzImageProperty> additionalProps = new List<WzImageProperty>();
        public List<WzImageProperty> additionalNonInfoProps = new List<WzImageProperty>();
        public string strMapName = "<Untitled>";
        public string strStreetName = "<Untitled>";
        public string strCategoryName = "HaCreator";
        public int id;

        //Editor related, not actual properties
        public MapType mapType = MapType.RegularMap;

        public WzImage Image { get; set; }

        public struct TimeMob
        {
            public int? startHour, endHour;
            public int id;
            public string message;

            public TimeMob(int? startHour, int? endHour, int id, string message)
            {
                this.startHour = startHour;
                this.endHour = endHour;
                this.id = id;
                this.message = message;
            }
        }
    }
}