using MapleLib.WzLib;
using NLog;
using RazzleServer.Data.WZ;
using RazzleServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Data
{
    public static class DataProvider
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

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

                    if(!img.Name.StartsWith("1"))
                    {
                        // ONLY LOAD SOME MAPS
                        // TODO - TEMPORARY
                        continue;
                    }
                    var info = img["info"];
                    var ladderRope = img["ladderRope"];
                    var portals = img["portal"];
                    var life = img["life"];

                    if(info != null)
                    {
                        if (info["link"] != null) //Linked map aka timed mini dungeon, we dont need that to load
                            continue;
                        var town = info["town"]?.ToInt() ?? 0;
                        map.Town = town == 1;
                        map.FieldType = info["fieldType"]?.ToInt() ?? 0;
                        map.FieldScript = info["fieldScript"]?.ToString();
                        map.FirstUserEnter = info["onFirstUserEnter"]?.ToString();
                        map.UserEnter = info["onUserEnter"]?.ToString();
                        map.Fly = info["fly"]?.ToInt() ?? 0;
                        map.Swim = info["swim"]?.ToInt() ?? 0;
                        map.ForcedReturn = info["forcedReturn"]?.ToInt() ?? 0;
                        map.ReturnMap = info["returnMap"]?.ToInt() ?? 0;
                        map.TimeLimit = info["timeLimit"]?.ToInt() ?? 0;
                        map.MobRate = info["mobRate"]?.ToDouble() ?? 1;
                        map.Limit = (WzMap.FieldLimit)(info["fieldLimit"]?.ToInt() ?? 0);
                    }

                    if (life != null)
                    {
                        foreach (var child in life.WzProperties)
                        {
                            string Type = child["type"].ToString();
                            if (Type == "m")
                            {
                                WzMap.MobSpawn mobSpawn = new WzMap.MobSpawn();
                                mobSpawn.MobId = child["id"].ToInt();
                                mobSpawn.wzMob = DataBuffer.GetMobById(mobSpawn.MobId);
                                if (mobSpawn.wzMob == null)
                                {
                                    Log.Error($"WzMob not found for mob [{mobSpawn.MobId}] on map [{map.MapId}]");
                                }
                                mobSpawn.Position = new Point(child["x"].ToInt(), child["y"].ToInt());
                                int mobTime = child["mobTime"]?.ToInt() ?? 0;
                                mobSpawn.MobTime = mobTime < 0 ? -1 : mobTime * 1000; //mobTime is in seconds in the .WZ
                                mobSpawn.Rx0 = (short)child["rx0"].ToInt();
                                mobSpawn.Rx1 = (short)child["rx1"].ToInt();
                                mobSpawn.Cy = (short)child["cy"].ToInt();
                                mobSpawn.Fh = (short)child["fh"].ToInt();
                                int F = child["f"]?.ToInt() ?? 0;
                                mobSpawn.F = (F == 1 ? false : true);
                                int Hide = child["hide"]?.ToInt() ?? 0;
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

                            portal.Type = (WzMap.PortalType)childNode["pt"].ToInt(0);
                            if (portal.Type == WzMap.PortalType.TownportalPoint)
                            {
                                portal.Id = townPortal;
                                townPortal++;
                            }

                            portal.Position = new Point((int)childNode["x"].WzValue, (int)childNode["y"].WzValue);
                            portal.ToMap = childNode["tm"].ToInt(0);
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
            var file = new WzFile(path, (short)ServerConfig.Instance.Version, WzMapleVersion.GMS);
            file.ParseWzFile();
            int ret = 0;

            foreach (var imgNode in file.WzImages)
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
                Mob.Level = mobInfo["level"]?.ToInt() ?? 1;
                Mob.HP = mobInfo["maxHP"]?.ToInt() ?? 1;
                Mob.MP = mobInfo["maxMP"]?.ToInt() ?? 1;
                Mob.Speed = mobInfo["speed"]?.ToInt() ?? 0;
                Mob.Kb = mobInfo["pushed"]?.ToInt() ?? 0;
                Mob.PAD = mobInfo["PADamage"]?.ToInt() ?? 0;
                Mob.PDD = mobInfo["PDDamage"]?.ToInt() ?? 0;
                Mob.PDRate = mobInfo["PDRate"]?.ToInt() ?? 0;
                Mob.MAD = mobInfo["MADamage"]?.ToInt() ?? 0;
                Mob.MDD = mobInfo["MDDamage"]?.ToInt() ?? 0;
                Mob.MDRate = mobInfo["MDRate"]?.ToInt() ?? 0;
                Mob.Eva = mobInfo["eva"]?.ToInt() ?? 0;
                Mob.Acc = mobInfo["acc"]?.ToInt() ?? 0;
                Mob.Exp = mobInfo["exp"]?.ToInt() ?? 0;
                Mob.SummonType = mobInfo["summonType"]?.ToInt() ?? 0;
                //Mob.Invincible = mobInfo["invincible"].ToInt();
                //Mob.FixedDamage = mobInfo["fixDamage"].ToInt(); ;
                //Mob.FFALoot = mobInfo["publicReward"].ToInt() > 0;
                //Mob.ExplosiveReward = mobInfo["explosiveReward"].ToInt() > 0;
                //Mob.IsBoss = mobInfo["boss"].ToInt() > 0;
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
            file.Dispose();
            return ret;
        }
    }
}
