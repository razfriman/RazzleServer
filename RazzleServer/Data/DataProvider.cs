using MapleLib.WzLib;
using Microsoft.Extensions.Logging;
using RazzleServer.Data.WZ;
using RazzleServer.Server;
using System.Drawing;
using System.Linq;
using RazzleServer.Util;
using MapleLib.WzLib.WzProperties;

namespace RazzleServer.Data
{
    public static class DataProvider
    {
        private static ILogger Log = LogManager.Log;

        public static int LoadMaps(string path)
        {
            var file = new WzFile(path, (short)ServerConfig.Instance.Version, WzMapleVersion.GMS);
            file.ParseWzFile();
            int ret = 0;

            foreach (var dir in (file["Map"] as WzDirectory).WzDirectories)
            {
                foreach (var img in dir.WzImages)
                {
                    WzMap map = new WzMap();

                    int mapId = int.Parse(img.Name.Replace(".img", ""));

                    var info = img["info"];
                    var ladderRope = img["ladderRope"];
                    var portals = img["portal"];
                    var life = img["life"];

                    if(info != null)
                    {
                        if (info["link"] != null) //Linked map aka timed mini dungeon, we dont need that to load
                            continue;
                        var town = info["town"]?.GetInt() ?? 0;
                        map.Town = town == 1;
                        map.FieldType = info["fieldType"]?.GetInt() ?? 0;
                        map.FieldScript = info["fieldScript"]?.ToString();
                        map.FirstUserEnter = info["onFirstUserEnter"]?.ToString();
                        map.UserEnter = info["onUserEnter"]?.ToString();
                        map.Fly = info["fly"]?.GetInt() ?? 0;
                        map.Swim = info["swim"]?.GetInt() ?? 0;
                        map.ForcedReturn = info["forcedReturn"]?.GetInt() ?? 0;
                        map.ReturnMap = info["returnMap"]?.GetInt() ?? 0;
                        map.TimeLimit = info["timeLimit"]?.GetInt() ?? 0;
                        map.MobRate = info["mobRate"]?.GetDouble() ?? 1;
                        map.Limit = (WzMap.FieldLimit)(info["fieldLimit"]?.GetInt() ?? 0);
                    }

                    if (life != null)
                    {
                        foreach (var child in life.WzProperties)
                        {
                            string Type = child["type"].ToString();
                            if (Type == "m")
                            {
                                WzMap.MobSpawn mobSpawn = new WzMap.MobSpawn();
                                mobSpawn.MobId = child["id"].GetInt();
                                mobSpawn.wzMob = DataBuffer.GetMobById(mobSpawn.MobId);
                                if (mobSpawn.wzMob == null)
                                {
                                    Log.LogError($"WzMob not found for mob [{mobSpawn.MobId}] on map [{map.MapId}]");
                                }
                                mobSpawn.Position = new Point(child["x"].GetInt(), child["y"].GetInt());
                                int mobTime = child["mobTime"]?.GetInt() ?? 0;
                                mobSpawn.MobTime = mobTime < 0 ? -1 : mobTime * 1000; //mobTime is in seconds in the .WZ
                                mobSpawn.Rx0 = (short)child["rx0"].GetInt();
                                mobSpawn.Rx1 = (short)child["rx1"].GetInt();
                                mobSpawn.Cy = (short)child["cy"].GetInt();
                                mobSpawn.Fh = (short)child["fh"].GetInt();
                                int F = child["f"]?.GetInt() ?? 0;
                                mobSpawn.F = (F == 1 ? false : true);
                                int Hide = child["hide"]?.GetInt() ?? 0;
                                mobSpawn.Hide = (Hide == 1);
                                map.MobSpawnPoints.Add(mobSpawn);
                            }
                        }
                    }

                            byte townPortal = 0x80;

                    if (portals != null)
                    {
                        foreach (var childNode in portals.WzProperties)
                        {
                            byte portalId = byte.Parse(childNode.Name);
                            WzMap.Portal portal = new WzMap.Portal();
                            portal.Id = portalId;

                            portal.Type = (WzMap.PortalType) ((WzIntProperty)childNode["pt"]).Value;
                            if (portal.Type == WzMap.PortalType.TownportalPoint)
                            {
                                portal.Id = townPortal;
                                townPortal++;
                            }

                            portal.Position = new Point((int)childNode["x"].WzValue, (int)childNode["y"].WzValue);
                            portal.ToMap = ((WzIntProperty)childNode["tm"]).Value;
                            portal.Name = childNode["pn"].ToString();
                            portal.ToName = childNode["tn"].ToString();

                            if (childNode["script"] != null)
                            {
                                portal.Script = childNode["script"].ToString();
                            }

                            if (!map.Portals.ContainsKey(portal.Name))
                            {
                                map.Portals.Add(portal.Name, portal);
                            }
                        }
                    }

                    if(!DataBuffer.MapBuffer.ContainsKey(mapId))
                    {
                        DataBuffer.MapBuffer[mapId] = map;
                        ret++;
                    }
                }
            }
            return ret;
        }

        public static int LoadMobs(string path)
        {
            using (var file = new WzFile(path, (short)ServerConfig.Instance.Version, WzMapleVersion.GMS))
            {
                file.ParseWzFile();
                file.WzDirectory.ParseImages();

                int ret = 0;

                foreach (var imgNode in file.WzDirectory.WzImages)
                {
                    if (!(imgNode["info"] != null && imgNode.Name.Contains(".img")))
                        continue;

                    int start = 0;
                    if (imgNode.Name.StartsWith("0")) start = 1;

                    int MobId = int.Parse(imgNode.Name.Substring(start, 7 - start));

                    if (DataBuffer.MobBuffer.ContainsKey(MobId))
                        continue;

                    var mobInfo = imgNode["info"];

                    WzMob Mob = new WzMob();
                    Mob.MobId = MobId;
                    Mob.Level = mobInfo["level"]?.GetInt() ?? 1;
                    Mob.HP = mobInfo["maxHP"]?.GetInt() ?? 1;
                    Mob.MP = mobInfo["maxMP"]?.GetInt() ?? 1;
                    //Mob.Speed = mobInfo["speed"]?.GetInt() ?? 0;
                    //Mob.Kb = mobInfo["pushed"]?.GetInt() ?? 0;
                    //Mob.PAD = mobInfo["PADamage"]?.GetInt() ?? 0;
                    //Mob.PDD = mobInfo["PDDamage"]?.GetInt() ?? 0;
                    //Mob.PDRate = mobInfo["PDRate"]?.GetInt() ?? 0;
                    //Mob.MAD = mobInfo["MADamage"]?.GetInt() ?? 0;
                    //Mob.MDD = mobInfo["MDDamage"]?.GetInt() ?? 0;
                    //Mob.MDRate = mobInfo["MDRate"]?.GetInt() ?? 0;
                    //Mob.Eva = mobInfo["eva"]?.GetInt() ?? 0;
                    //Mob.Acc = mobInfo["acc"]?.GetInt() ?? 0;
                    //Mob.Exp = mobInfo["exp"]?.GetInt() ?? 0;
                    //Mob.SummonType = mobInfo["summonType"]?.GetInt() ?? 0;


                    //Mob.Invincible = mobInfo["invincible"].GetInt();
                    //Mob.FixedDamage = mobInfo["fixDamage"].GetInt(); ;
                    //Mob.FFALoot = mobInfo["publicReward"].GetInt() > 0;
                    //Mob.ExplosiveReward = mobInfo["explosiveReward"].GetInt() > 0;
                    //Mob.IsBoss = mobInfo["boss"].GetInt() > 0;
                    //if (Info.ContainsChild("skill"))
                    //{
                    //    NXNode skill = Info.GetChild("skill");
                    //    for (int i = 0; skill.ContainsChild(i.ToString()); i++)
                    //    {
                    //        NXNode child = skill.GetChild(i.ToString());
                    //        Mob.Skills.Add(MobSkill.GetSkill(GetIntFromChild(child, "skill"), GetIntFromChild(child, "level")));
                    //    }
                    //}
                    DataBuffer.MobBuffer.Add(MobId, Mob);
                    ret++;
                }
            return ret;
			}

		}
    }
}
