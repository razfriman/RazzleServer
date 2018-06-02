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
        private static readonly ILogger Log = LogManager.Log;

        public static MapInfo Default = new MapInfo();

        public MapInfo()
        {
        }

        public MapInfo(WzImage image, string strMapName, string strStreetName, string strCategoryName)
        {
            Image = image;
            StrMapName = strMapName;
            StrStreetName = strStreetName;
            StrCategoryName = strCategoryName;
            foreach (var prop in image["info"].WzProperties)
            {
                switch (prop.Name)
                {
                    case "bgm":
                        Bgm = InfoTool.GetString(prop);
                        break;
                    case "cloud":
                        Cloud = InfoTool.GetBool(prop);
                        break;
                    case "swim":
                        Swim = InfoTool.GetBool(prop);
                        break;
                    case "forcedReturn":
                        ForcedReturn = InfoTool.GetInt(prop);
                        break;
                    case "hideMinimap":
                        HideMinimap = InfoTool.GetBool(prop);
                        break;
                    case "mapDesc":
                        MapDesc = InfoTool.GetString(prop);
                        break;
                    case "mapName":
                        MapName = InfoTool.GetString(prop);
                        break;
                    case "mapMark":
                        MapMark = InfoTool.GetString(prop);
                        break;
                    case "mobRate":
                        MobRate = InfoTool.GetFloat(prop);
                        break;
                    case "moveLimit":
                        MoveLimit = InfoTool.GetInt(prop);
                        break;
                    case "returnMap":
                        ReturnMap = InfoTool.GetInt(prop);
                        break;
                    case "town":
                        Town = InfoTool.GetBool(prop);
                        break;
                    case "version":
                        Version = InfoTool.GetInt(prop);
                        break;
                    case "fieldLimit":
                        var fl = InfoTool.GetInt(prop);
                        if (fl >= (int)Math.Pow(2, 23))
                        {
                            Log.LogError($"Invalid fieldlimit: {fl}");
                            fl = fl & ((int)Math.Pow(2, 23) - 1);
                        }
                        FieldLimit = (FieldLimit)fl;
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
                        TimeLimit = InfoTool.GetInt(prop);
                        break;
                    case "lvLimit":
                        LvLimit = InfoTool.GetInt(prop);
                        break;
                    case "onFirstUserEnter":
                        OnFirstUserEnter = InfoTool.GetString(prop);
                        break;
                    case "onUserEnter":
                        OnUserEnter = InfoTool.GetString(prop);
                        break;
                    case "fly":
                        Fly = InfoTool.GetBool(prop);
                        break;
                    case "noMapCmd":
                        NoMapCmd = InfoTool.GetBool(prop);
                        break;
                    case "partyOnly":
                        PartyOnly = InfoTool.GetBool(prop);
                        break;
                    case "fieldType":
                        var ft = InfoTool.GetInt(prop);
                        if (!Enum.IsDefined(typeof(FieldType), ft))
                        {
                            Log.LogError($"Invalid fieldType: {ft}");
                            ft = 0;
                        }
                        FieldType = (FieldType)ft;
                        break;
                    case "miniMapOnOff":
                        MiniMapOnOff = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffle":
                        ReactorShuffle = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffleName":
                        ReactorShuffleName = InfoTool.GetString(prop);
                        break;
                    case "personalShop":
                        PersonalShop = InfoTool.GetBool(prop);
                        break;
                    case "entrustedShop":
                        EntrustedShop = InfoTool.GetBool(prop);
                        break;
                    case "effect":
                        Effect = InfoTool.GetString(prop);
                        break;
                    case "lvForceMove":
                        LvForceMove = InfoTool.GetInt(prop);
                        break;
                    case "timeMob":
                        var startHour = InfoTool.GetOptionalInt(prop["startHour"]);
                        var endHour = InfoTool.GetOptionalInt(prop["endHour"]);
                        var propId = InfoTool.GetOptionalInt(prop["id"]);
                        var message = InfoTool.GetOptionalString(prop["message"]);
                        if (propId == null || message == null || startHour == null ^ endHour == null)
                        {
                            Log.LogError("timeMob is missing data");
                        }
                        else
                        {
                            TimeMob = new TimeMob(startHour, endHour, (int)propId, message);
                        }

                        break;
                    case "help":
                        Help = InfoTool.GetString(prop);
                        break;
                    case "snow":
                        Snow = InfoTool.GetBool(prop);
                        break;
                    case "rain":
                        Rain = InfoTool.GetBool(prop);
                        break;
                    case "dropExpire":
                        DropExpire = InfoTool.GetInt(prop);
                        break;
                    case "decHP":
                        DecHp = InfoTool.GetInt(prop);
                        break;
                    case "decInterval":
                        DecInterval = InfoTool.GetInt(prop);
                        break;
                    case "expeditionOnly":
                        ExpeditionOnly = InfoTool.GetBool(prop);
                        break;
                    case "fs":
                        Fs = InfoTool.GetFloat(prop);
                        break;
                    case "protectItem":
                        ProtectItem = InfoTool.GetInt(prop);
                        break;
                    case "createMobInterval":
                        CreateMobInterval = InfoTool.GetInt(prop);
                        break;
                    case "fixedMobCapacity":
                        FixedMobCapacity = InfoTool.GetInt(prop);
                        break;
                    case "streetName":
                        StreetName = InfoTool.GetString(prop);
                        break;
                    case "noRegenMap":
                        NoRegenMap = InfoTool.GetBool(prop);
                        break;
                    case "allowedItem":
                        AllowedItem = new List<int>();
                        if (prop.WzProperties != null && prop.WzProperties.Count > 0)
                        {
                            foreach (var item in prop.WzProperties)
                            {
                                AllowedItem.Add(item.GetInt());
                            }
                        }

                        break;
                    case "recovery":
                        Recovery = InfoTool.GetFloat(prop);
                        break;
                    case "blockPBossChange":
                        BlockPBossChange = InfoTool.GetBool(prop);
                        break;
                    case "everlast":
                        Everlast = InfoTool.GetBool(prop);
                        break;
                    case "damageCheckFree":
                        DamageCheckFree = InfoTool.GetBool(prop);
                        break;
                    case "dropRate":
                        DropRate = InfoTool.GetFloat(prop);
                        break;
                    case "scrollDisable":
                        ScrollDisable = InfoTool.GetBool(prop);
                        break;
                    case "needSkillForFly":
                        NeedSkillForFly = InfoTool.GetBool(prop);
                        break;
                    case "zakum2Hack":
                        Zakum2Hack = InfoTool.GetBool(prop);
                        break;
                    case "allMoveCheck":
                        AllMoveCheck = InfoTool.GetBool(prop);
                        break;
                    case "VRLimit":
                        VrLimit = InfoTool.GetBool(prop);
                        break;
                    case "consumeItemCoolTime":
                        ConsumeItemCoolTime = InfoTool.GetBool(prop);
                        break;
                    default:
                        Log.LogWarning($"Unknown property: {prop.Name}");
                        AdditionalProps.Add(prop.DeepClone());
                        break;
                }
            }
        }

        public static Rectangle GetVr(WzImage image)
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

        public void Save(WzImage dest, Rectangle vr)
        {
            var info = new WzSubProperty
            {
                ["bgm"] = InfoTool.SetString(Bgm),
                ["cloud"] = InfoTool.SetBool(Cloud),
                ["swim"] = InfoTool.SetBool(Swim),
                ["forcedReturn"] = InfoTool.SetInt(ForcedReturn),
                ["hideMinimap"] = InfoTool.SetBool(HideMinimap),
                ["mapDesc"] = InfoTool.SetOptionalString(MapDesc),
                ["mapName"] = InfoTool.SetOptionalString(MapDesc),
                ["mapMark"] = InfoTool.SetString(MapMark),
                ["mobRate"] = InfoTool.SetFloat(MobRate),
                ["moveLimit"] = InfoTool.SetOptionalInt(MoveLimit),
                ["returnMap"] = InfoTool.SetInt(ReturnMap),
                ["town"] = InfoTool.SetBool(Town),
                ["version"] = InfoTool.SetInt(Version),
                ["fieldLimit"] = InfoTool.SetInt((int) FieldLimit),
                ["timeLimit"] = InfoTool.SetOptionalInt(TimeLimit),
                ["lvLimit"] = InfoTool.SetOptionalInt(LvLimit),
                ["onFirstUserEnter"] = InfoTool.SetOptionalString(OnFirstUserEnter),
                ["onUserEnter"] = InfoTool.SetOptionalString(OnUserEnter),
                ["fly"] = InfoTool.SetOptionalBool(Fly),
                ["noMapCmd"] = InfoTool.SetOptionalBool(NoMapCmd),
                ["partyOnly"] = InfoTool.SetOptionalBool(PartyOnly),
                ["fieldType"] = InfoTool.SetOptionalInt((int?) FieldType),
                ["miniMapOnOff"] = InfoTool.SetOptionalBool(MiniMapOnOff),
                ["reactorShuffle"] = InfoTool.SetOptionalBool(ReactorShuffle),
                ["reactorShuffleName"] = InfoTool.SetOptionalString(ReactorShuffleName),
                ["personalShop"] = InfoTool.SetOptionalBool(PersonalShop),
                ["entrustedShop"] = InfoTool.SetOptionalBool(EntrustedShop),
                ["effect"] = InfoTool.SetOptionalString(Effect),
                ["lvForceMove"] = InfoTool.SetOptionalInt(LvForceMove)
            };
            if (TimeMob != null)
            {
                var prop = new WzSubProperty
                {
                    ["startHour"] = InfoTool.SetOptionalInt(TimeMob.Value.StartHour),
                    ["endHour"] = InfoTool.SetOptionalInt(TimeMob.Value.EndHour),
                    ["id"] = InfoTool.SetOptionalInt(TimeMob.Value.ID),
                    ["message"] = InfoTool.SetOptionalString(TimeMob.Value.Message)
                };
                info["timeMob"] = prop;
            }
            info["help"] = InfoTool.SetOptionalString(Help);
            info["snow"] = InfoTool.SetOptionalBool(Snow);
            info["rain"] = InfoTool.SetOptionalBool(Rain);
            info["dropExpire"] = InfoTool.SetOptionalInt(DropExpire);
            info["decHP"] = InfoTool.SetOptionalInt(DecHp);
            info["decInterval"] = InfoTool.SetOptionalInt(DecInterval);
            info["expeditionOnly"] = InfoTool.SetOptionalBool(ExpeditionOnly);
            info["fs"] = InfoTool.SetOptionalFloat(Fs);
            info["protectItem"] = InfoTool.SetOptionalInt(ProtectItem);
            info["createMobInterval"] = InfoTool.SetOptionalInt(CreateMobInterval);
            info["fixedMobCapacity"] = InfoTool.SetOptionalInt(FixedMobCapacity);
            info["streetName"] = InfoTool.SetOptionalString(StreetName);
            info["noRegenMap"] = InfoTool.SetOptionalBool(NoRegenMap);
            if (AllowedItem != null)
            {
                var prop = new WzSubProperty();
                for (var i = 0; i < AllowedItem.Count; i++)
                {
                    prop[i.ToString()] = InfoTool.SetInt(AllowedItem[i]);
                }
                info["allowedItem"] = prop;
            }
            info["recovery"] = InfoTool.SetOptionalFloat(Recovery);
            info["blockPBossChange"] = InfoTool.SetOptionalBool(BlockPBossChange);
            info["everlast"] = InfoTool.SetOptionalBool(Everlast);
            info["damageCheckFree"] = InfoTool.SetOptionalBool(DamageCheckFree);
            info["dropRate"] = InfoTool.SetOptionalFloat(DropRate);
            info["scrollDisable"] = InfoTool.SetOptionalBool(ScrollDisable);
            info["needSkillForFly"] = InfoTool.SetOptionalBool(NeedSkillForFly);
            info["zakum2Hack"] = InfoTool.SetOptionalBool(Zakum2Hack);
            info["allMoveCheck"] = InfoTool.SetOptionalBool(AllMoveCheck);
            info["VRLimit"] = InfoTool.SetOptionalBool(VrLimit);
            info["consumeItemCoolTime"] = InfoTool.SetOptionalBool(ConsumeItemCoolTime);
            foreach (var prop in AdditionalProps)
            {
                info.AddProperty(prop);
            }
            if (vr != null)
            {
                info["VRLeft"] = InfoTool.SetInt(vr.Lt.X);
                info["VRRight"] = InfoTool.SetInt(vr.Rb.X);
                info["VRTop"] = InfoTool.SetInt(vr.Lt.Y);
                info["VRBottom"] = InfoTool.SetInt(vr.Rb.Y);
            }
            dest["info"] = info;
        }

        public readonly int Version = 10;

        //Must have
        public string Bgm = "Bgm00/GoPicnic";
        public string MapMark = "None";
        public FieldLimit FieldLimit = FieldLimit.FIELDOPT_NONE;
        public int ReturnMap = 999999999;
        public int ForcedReturn = 999999999;
        public bool Cloud;
        public bool Swim;
        public bool HideMinimap;
        public bool Town;
        public float MobRate = 1.5f;

        //Optional
        //public int link = -1;
        public int? TimeLimit;
        public int? LvLimit;
        public FieldType? FieldType;
        public string OnFirstUserEnter;
        public string OnUserEnter;
        public MapleBool Fly = null;
        public MapleBool NoMapCmd = null;
        public MapleBool PartyOnly = null;
        public MapleBool ReactorShuffle = null;
        public string ReactorShuffleName;
        public MapleBool PersonalShop = null;
        public MapleBool EntrustedShop = null;
        public string Effect; //Bubbling; 610030550 and many others
        public int? LvForceMove; //limit FROM value
        public TimeMob? TimeMob;
        public string Help; //help string
        public MapleBool Snow = null;
        public MapleBool Rain = null;
        public int? DropExpire; //in seconds
        public int? DecHp;
        public int? DecInterval;
        public MapleBool ExpeditionOnly = null;
        public float? Fs; //slip on ice speed, default 0.2
        public int? ProtectItem; //Id, item protecting from cold
        public int? CreateMobInterval; //used for massacre pqs
        public int? FixedMobCapacity; //mob capacity to target (used for massacre pqs)

        //Unknown optional
        public int? MoveLimit;
        public string MapDesc;
        public string MapName;
        public string StreetName;
        public MapleBool MiniMapOnOff = null;
        public MapleBool NoRegenMap = null; //610030400
        public List<int> AllowedItem;
        public float? Recovery; //recovery rate, like in sauna (3)
        public MapleBool BlockPBossChange = null; //something with monster carnival
        public MapleBool Everlast = null; //something with bonus stages of PQs
        public MapleBool DamageCheckFree = null; //something with fishing event
        public float? DropRate;
        public MapleBool ScrollDisable = null;
        public MapleBool NeedSkillForFly = null;
        public MapleBool Zakum2Hack = null; //JQ hack protection
        public MapleBool AllMoveCheck = null; //another JQ hack protection
        public MapleBool VrLimit = null; //use vr's as limits?
        public MapleBool ConsumeItemCoolTime = null; //cool time of consume item

        //Special
        public List<WzImageProperty> AdditionalProps = new List<WzImageProperty>();
        public List<WzImageProperty> AdditionalNonInfoProps = new List<WzImageProperty>();
        public string StrMapName = "<Untitled>";
        public string StrStreetName = "<Untitled>";
        public string StrCategoryName = "HaCreator";
        public int ID;

        //Editor related, not actual properties
        public MapType MapType = MapType.RegularMap;

        public WzImage Image { get; set; }
    }
    
    public struct TimeMob
    {
        public int? StartHour, EndHour;
        public readonly int ID;
        public readonly string Message;

        public TimeMob(int? startHour, int? endHour, int id, string message)
        {
            StartHour = startHour;
            EndHour = endHour;
            ID = id;
            Message = message;
        }
    }
}