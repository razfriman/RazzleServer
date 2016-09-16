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
                    //Log.Info(img.Name);

                    WzMap map = new WzMap();

                    int mapId = int.Parse(img.Name.Replace(".img", ""));

                    if(!img.Name.StartsWith("1"))
                    {
                        continue;
                    }
                    var info = img["info"];
                    var ladderRope = img["ladderRope"];
                    var portals = img["portal"];

                    byte townPortal = 0x80;

                    if (portals != null)
                    {
                        foreach (var childNode in portals.WzProperties)
                        {
                            byte portalId = byte.Parse(childNode.Name);
                            WzMap.Portal portal = new WzMap.Portal();
                            portal.Id = portalId;

                            portal.Type = (WzMap.PortalType)(int)childNode["pt"].WzValue;
                            if (portal.Type == WzMap.PortalType.TownportalPoint)
                            {
                                portal.Id = townPortal;
                                townPortal++;
                            }

                            portal.Position = new Point((int)childNode["x"].WzValue, (int)childNode["y"].WzValue);
                            portal.ToMap = (int)childNode["tm"].WzValue;
                            portal.Name = (string)childNode["pn"].WzValue;
                            portal.ToName = (string)childNode["tn"];

                            if (childNode["script"] != null)
                            {
                                portal.Script = (string)childNode["script"];
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

        //        if (mapImg.ContainsChild("info"))
        //        {
        //            info = mapImg.GetChild("info");
        //            if (info.ContainsChild("link")) //Linked map aka timed mini dungeon, we dont need that to load
        //                continue;
        //            map.Town = GetIntFromChild(info, "town") == 1;

        //            map.FieldType = GetIntFromChild(info, "fieldType");
        //            map.FieldScript = GetStringFromChild(info, "fieldScript");
        //            map.FirstUserEnter = GetStringFromChild(info, "onFirstUserEnter");
        //            map.UserEnter = GetStringFromChild(info, "onUserEnter");
        //            map.Fly = GetIntFromChild(info, "fly");
        //            map.Swim = GetIntFromChild(info, "swim");
        //            map.ForcedReturn = GetIntFromChild(info, "forcedReturn");
        //            map.ReturnMap = GetIntFromChild(info, "returnMap");
        //            map.TimeLimit = GetIntFromChild(info, "timeLimit");
        //            map.MobRate = GetDoubleFromChild(info, "mobRate");
        //            map.Limit = (WzMap.FieldLimit)GetIntFromChild(info, "fieldLimit");
        //        }
        //        if (mapImg.ContainsChild("ladderRope"))
        //        {
        //            info = mapImg.GetChild("ladderRope");
        //            foreach (NXNode ChildNode in info)
        //            {
        //                WzMap.LadderRope lr = new WzMap.LadderRope();
        //                lr.StartPoint = new Point(GetIntFromChild(ChildNode, "x"), GetIntFromChild(ChildNode, "y1"));
        //                lr.EndPoint = new Point(GetIntFromChild(ChildNode, "x"), GetIntFromChild(ChildNode, "y2"));
        //                map.LaderRopes.Add(lr);
        //            }
        //        }
        //        if (mapImg.ContainsChild("life"))
        //        {
        //            info = mapImg.GetChild("life");
        //            foreach (NXNode ChildNode in info)
        //            {
        //                string Type = GetStringFromChild(ChildNode, "type");
        //                if (Type == "m")
        //                {
        //                    WzMap.MobSpawn mobSpawn = new WzMap.MobSpawn();
        //                    mobSpawn.MobId = GetIntFromChild(ChildNode, "id");
        //                    mobSpawn.wzMob = DataBuffer.GetMobById(mobSpawn.MobId);
        //                    if (mobSpawn.wzMob == null)
        //                    {
        //                        ServerConsole.Error("WzMob not found for mob " + mobSpawn.MobId + " on map " + map.MapId);
        //                    }
        //                    mobSpawn.Position = new Point(GetIntFromChild(ChildNode, "x"), GetIntFromChild(ChildNode, "y"));
        //                    int mobTime = GetIntFromChild(ChildNode, "mobTime");
        //                    mobSpawn.MobTime = mobTime < 0 ? -1 : mobTime * 1000; //mobTime is in seconds in the .WZ
        //                    mobSpawn.Rx0 = (short)GetIntFromChild(ChildNode, "rx0");
        //                    mobSpawn.Rx1 = (short)GetIntFromChild(ChildNode, "rx1");
        //                    mobSpawn.Cy = (short)GetIntFromChild(ChildNode, "cy");
        //                    mobSpawn.Fh = (short)GetIntFromChild(ChildNode, "fh");
        //                    int F = GetIntFromChild(ChildNode, "f");
        //                    mobSpawn.F = (F == 1 ? false : true);
        //                    int Hide = GetIntFromChild(ChildNode, "hide");
        //                    mobSpawn.Hide = (Hide == 1);
        //                    map.MobSpawnPoints.Add(mobSpawn);
        //                }
        //                else
        //                {
        //                    WzMap.Npc Npc = new WzMap.Npc();
        //                    Npc.Id = GetIntFromChild(ChildNode, "id");
        //                    Npc.x = (short)GetIntFromChild(ChildNode, "x");
        //                    Npc.y = (short)GetIntFromChild(ChildNode, "y");
        //                    Npc.Rx0 = (short)GetIntFromChild(ChildNode, "rx0");
        //                    Npc.Rx1 = (short)GetIntFromChild(ChildNode, "rx1");
        //                    Npc.Cy = (short)GetIntFromChild(ChildNode, "cy");
        //                    Npc.Fh = (short)GetIntFromChild(ChildNode, "fh");
        //                    int F = GetIntFromChild(ChildNode, "f");
        //                    Npc.F = (F == 1 ? false : true);
        //                    int Hide = GetIntFromChild(ChildNode, "hide");
        //                    if (ChildNode.ContainsChild("limitedname")) //No special event npcs for now
        //                        Hide = 1;
        //                    Npc.Hide = (Hide == 1);
        //                    map.Npcs.Add(Npc);
        //                }
        //            }
        //        }

        //      
        //        short topBound = 0;
        //        short bottomBound = 0;
        //        short leftBound = 0;
        //        short rightBound = 0;

        //        List<WzMap.FootHold> footHolds = new List<WzMap.FootHold>();

        //        if (mapImg.ContainsChild("foothold"))
        //        {
        //            foreach (NXNode fhRoot in mapImg.GetChild("foothold"))
        //            {
        //                foreach (NXNode fhCat in fhRoot)
        //                {
        //                    foreach (NXNode fh in fhCat)
        //                    {
        //                        WzMap.FootHold footHold = new WzMap.FootHold();

        //                        footHold.Id = short.Parse(fh.Name);
        //                        footHold.Next = (short)GetIntFromChild(fh, "next");
        //                        footHold.Prev = (short)GetIntFromChild(fh, "prev");
        //                        Point p1 = new Point(GetIntFromChild(fh, "x1"), GetIntFromChild(fh, "y1"));
        //                        Point p2 = new Point(GetIntFromChild(fh, "x2"), GetIntFromChild(fh, "y2"));
        //                        footHold.Point1 = p1;
        //                        footHold.Point2 = p2;
        //                        footHolds.Add(footHold);

        //                        if (p1.X < leftBound)
        //                            leftBound = (short)p1.X;
        //                        if (p2.X < leftBound)
        //                            leftBound = (short)p2.X;

        //                        if (p1.X > rightBound)
        //                            rightBound = (short)p1.X;
        //                        if (p2.X > rightBound)
        //                            rightBound = (short)p2.X;

        //                        if (p1.Y > bottomBound)
        //                            bottomBound = (short)p1.Y;
        //                        if (p2.Y > bottomBound)
        //                            bottomBound = (short)p2.Y;

        //                        if (p1.Y < topBound)
        //                            topBound = (short)p1.Y;
        //                        if (p2.Y < topBound)
        //                            topBound = (short)p2.Y;

        //                    }
        //                }
        //            }
        //        }
        //        map.FootHolds = footHolds;

        //        map.TopBorder = topBound;
        //        map.BottomBorder = bottomBound;
        //        map.LeftBorder = leftBound;
        //        map.RightBorder = rightBound;

        //        if (mapImg.ContainsChild("reactor"))
        //        {
        //            info = mapImg.GetChild("reactor");
        //            foreach (NXNode childNode in info)
        //            {
        //                WzMap.Reactor Reactor = new WzMap.Reactor();
        //                Reactor.Position = new Point(GetIntFromChild(childNode, "x"), GetIntFromChild(childNode, "y"));
        //                Reactor.Id = GetIntFromChild(childNode, "id");
        //                Reactor.ReactorTime = GetIntFromChild(childNode, "reactorTime");
        //                map.Reactors.Add(Reactor);
        //            }
        //        }

        //        map.Reactors.AddRange(GenerateRandomVeins(map));
        //        lock (DataBuffer.MapBuffer)
        //        {
        //            DataBuffer.MapBuffer.Add(mapId, map);
        //        }
        //    }
        //    wrap.ResetEvent.Set();
        //}

        //private static List<WzMap.Reactor> GenerateRandomVeins(WzMap map)
        //{
        //    List<WzMap.Reactor> Veins = new List<WzMap.Reactor>();
        //    if (map.Town || map.MobSpawnPoints.Count == 0) return Veins;
        //    //Herbs and ores calculations
        //    int avgMobLvl = (int)Math.Floor(map.MobSpawnPoints.Average(x => x.wzMob.Level));
        //    Dictionary<int, int> Ores = new Dictionary<int, int>();
        //    Dictionary<int, int> Herbs = new Dictionary<int, int>();
        //    if (avgMobLvl > 0 && avgMobLvl <= 60)
        //    {
        //        Ores.Add(200000, 400);
        //        Ores.Add(200001, 400);
        //        Ores.Add(200002, 120);
        //        Ores.Add(200003, 80);

        //        Herbs.Add(100000, 400);
        //        Herbs.Add(100001, 400);
        //        Herbs.Add(100002, 120);
        //        Herbs.Add(100003, 80);
        //    }
        //    else if (avgMobLvl > 60 && avgMobLvl <= 120)
        //    {
        //        Ores.Add(200002, 400);
        //        Ores.Add(200003, 300);
        //        Ores.Add(200004, 200);
        //        Ores.Add(200005, 100);

        //        Herbs.Add(100002, 400);
        //        Herbs.Add(100003, 300);
        //        Herbs.Add(100004, 200);
        //        Herbs.Add(100005, 100);
        //    }
        //    else if (avgMobLvl > 120 && avgMobLvl <= 150)
        //    {
        //        Ores.Add(200005, 200);
        //        Ores.Add(200006, 200);
        //        Ores.Add(200007, 200);
        //        Ores.Add(200008, 150);
        //        Ores.Add(200009, 150);
        //        Ores.Add(200011, 100);

        //        Herbs.Add(100005, 200);
        //        Herbs.Add(100006, 200);
        //        Herbs.Add(100007, 200);
        //        Herbs.Add(100008, 150);
        //        Herbs.Add(100009, 150);
        //        Herbs.Add(100011, 100);
        //    }
        //    else if (avgMobLvl > 150)
        //    {
        //        Ores.Add(200008, 200);
        //        Ores.Add(200009, 200);
        //        Ores.Add(200011, 200);
        //        Ores.Add(200012, 200);
        //        Ores.Add(200013, 200);

        //        Herbs.Add(100008, 200);
        //        Herbs.Add(100009, 200);
        //        Herbs.Add(100011, 200);
        //        Herbs.Add(100012, 200);
        //        Herbs.Add(100013, 200);
        //    }

        //    //MapId as seed to make the positions and spawn seem static per map
        //    List<int> PassedFhs = new List<int>();
        //    Random RandomCalc = new Random(map.MapId);
        //    for (int i = 0; i < 4; i++)
        //    {
        //        WzMap.Reactor RandomOre = new WzMap.Reactor();
        //        WzMap.Reactor RandomHerb = new WzMap.Reactor();

        //        //Todo: fix near edge positions
        //        if (i + 1 == map.MobSpawnPoints.Count || PassedFhs.Count == map.MobSpawnPoints.Count) break;
        //        int RandomFH = RandomCalc.Next(0, map.MobSpawnPoints.Count);
        //        for (int x = 0; PassedFhs.Contains(RandomFH); x++)
        //        {
        //            RandomFH = RandomCalc.Next(0, map.MobSpawnPoints.Count);
        //            if (x == 10) RandomFH = -1;
        //        }
        //        if (RandomFH == -1) break;
        //        WzMap.FootHold OreFh = GetFootHoldBelow(map.MobSpawnPoints[RandomFH].Position, map);

        //        RandomFH = RandomCalc.Next(0, map.MobSpawnPoints.Count);
        //        for (int x = 0; PassedFhs.Contains(RandomFH); x++)
        //        {
        //            RandomFH = RandomCalc.Next(0, map.MobSpawnPoints.Count);
        //            if (x == 10) RandomFH = -1;
        //        }
        //        if (RandomFH == -1) break;
        //        WzMap.FootHold HerbFh = GetFootHoldBelow(map.MobSpawnPoints[RandomFH].Position, map);

        //        Dictionary<int, int> Temp = new Dictionary<int, int>();
        //        if (OreFh == null && HerbFh == null) continue;
        //        if (OreFh != null)
        //        {
        //            Point RandomPos = OreFh.Point1;
        //            RandomOre.Position = RandomPos;
        //            PassedFhs.Add(RandomFH);

        //            int OreTotal = Ores.Sum(x => x.Value);
        //            int prev = 0;
        //            foreach (KeyValuePair<int, int> Ore in Ores)
        //            {
        //                Temp.Add(Ore.Key, Ore.Value + prev);
        //                prev += Ore.Value;
        //            }
        //            Ores = Temp;

        //            int RandomOreId = RandomCalc.Next(0, OreTotal);
        //            RandomOre.Id = Ores.FirstOrDefault(x => x.Value >= RandomOreId).Key;

        //            int RandomOreReactorTime = 5;
        //            switch (RandomOre.Id % 100000)
        //            {
        //                case 6:
        //                case 7:
        //                case 8:
        //                case 9:
        //                    RandomOreReactorTime = 120;
        //                    break;
        //                case 11:
        //                case 12:
        //                    RandomOreReactorTime = 3600;
        //                    break;
        //                case 13:
        //                    RandomOreReactorTime = 18000;
        //                    break;
        //                default:
        //                    RandomOreReactorTime = 60;
        //                    break;
        //            }
        //            RandomOre.ReactorTime = RandomOreReactorTime;
        //            Veins.Add(RandomOre);
        //        }
        //        if (HerbFh != null)
        //        {
        //            Point RandomPos = HerbFh.Point1;
        //            RandomHerb.Position = RandomPos;
        //            PassedFhs.Add(RandomFH);

        //            int HerbTotal = Herbs.Sum(x => x.Value);
        //            Temp = new Dictionary<int, int>();
        //            int prev = 0;
        //            foreach (KeyValuePair<int, int> Herb in Herbs)
        //            {
        //                Temp.Add(Herb.Key, Herb.Value + prev);
        //                prev += Herb.Value;
        //            }
        //            Herbs = Temp;

        //            int RandomHerbId = RandomCalc.Next(0, HerbTotal);
        //            RandomHerb.Id = Herbs.FirstOrDefault(x => x.Value >= RandomHerbId).Key;

        //            int RandomHerbReactorTime = 5;
        //            switch (RandomHerb.Id % 100000)
        //            {
        //                case 6:
        //                case 7:
        //                case 8:
        //                case 9:
        //                    RandomHerbReactorTime = 120;
        //                    break;
        //                case 11:
        //                case 12:
        //                    RandomHerbReactorTime = 3600;
        //                    break;
        //                case 13:
        //                    RandomHerbReactorTime = 18000;
        //                    break;
        //                default:
        //                    RandomHerbReactorTime = 60;
        //                    break;
        //            }
        //            RandomHerb.ReactorTime = RandomHerbReactorTime;
        //            Veins.Add(RandomHerb);
        //        }
        //    }

        //    return Veins;
        //}

        //public static WzMap.FootHold GetFootHoldBelow(Point position, WzMap WzInfo)
        //{
        //    List<WzMap.FootHold> validXFootHolds = WzInfo.FootHolds.Where(fh => !fh.IsWall && position.X >= fh.Point1.X && position.X <= fh.Point2.X && (fh.Point1.Y > position.Y || fh.Point2.Y > position.Y)).ToList();
        //    if (validXFootHolds.Any())
        //    {
        //        foreach (WzMap.FootHold fh in validXFootHolds.OrderBy(fh => (fh.Point1.Y < fh.Point2.Y ? fh.Point1.Y : fh.Point2.Y)))
        //        {
        //            if (fh.Point1.Y != fh.Point2.Y) //diagonal foothold
        //            {
        //                int width = Math.Abs(fh.Point2.X - fh.Point1.X);
        //                int height = Math.Abs(fh.Point2.Y - fh.Point1.Y);
        //                double xy = (double)height / width;

        //                int distFromPoint1 = position.X - fh.Point1.X; //doesnt matter if abs or not as long as you use point1's Y too

        //                int addedY = (int)(distFromPoint1 * xy);

        //                int y = fh.Point1.Y + addedY; //Foothold's Y value on position.X

        //                if (y >= position.Y)
        //                    return fh;
        //            }
        //            else
        //            {
        //                return fh;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public static int LoadSkills(String path)
        //{
        //    NXFile File = new NXFile(path);
        //    int ret = 0;
        //    List<ManualResetEvent> waitHandles = new List<ManualResetEvent>();

        //    #region Player skills
        //    foreach (NXNode skillImg in File.BaseNode)
        //    {
        //        ManualResetEvent resetEvent = new ManualResetEvent(false);
        //        waitHandles.Add(resetEvent);
        //        NXNodeResetEventWrapper wrap = new NXNodeResetEventWrapper(skillImg, resetEvent);
        //        ThreadPool.QueueUserWorkItem(new WaitCallback(LoadCharacterSkill), wrap);
        //    }
        //    #endregion

        //    #region Familiar skills
        //    NXNode familiarImg = File.BaseNode.GetChild("FamiliarSkill.img");
        //    foreach (NXNode familiarSkill in familiarImg)
        //    {
        //        int skillId;
        //        if (!int.TryParse(familiarSkill.Name, out skillId) || DataBuffer.FamiliarSkillBuffer.ContainsKey(skillId))
        //            continue;
        //        WzFamiliarSkill newFamiliarSkill = new WzFamiliarSkill();
        //        newFamiliarSkill.Prop = GetIntFromChild(familiarSkill, "prop", 0);
        //        newFamiliarSkill.AttackCount = GetIntFromChild(familiarSkill, "attackCount", 1);
        //        newFamiliarSkill.TargetCount = GetIntFromChild(familiarSkill, "targetCount", 1);
        //        newFamiliarSkill.Time = GetIntFromChild(familiarSkill, "time", 0);
        //        newFamiliarSkill.Speed = GetIntFromChild(familiarSkill, "speed", 1);
        //        newFamiliarSkill.Knockback = (GetIntFromChild(familiarSkill, "knockback", 0) > 0 || GetIntFromChild(familiarSkill, "attract", 0) > 0);
        //        //TODO: status effects

        //        DataBuffer.FamiliarSkillBuffer.Add(skillId, newFamiliarSkill);
        //        ret++;

        //    }
        //    #endregion

        //    #region Crafting Recipies  
        //    NXNode[] RecipeNodes = new NXNode[] {
        //        File.BaseNode.GetChild("Recipe_9200.img"),
        //        File.BaseNode.GetChild("Recipe_9201.img"),
        //        File.BaseNode.GetChild("Recipe_9202.img"),
        //        File.BaseNode.GetChild("Recipe_9203.img"),
        //        File.BaseNode.GetChild("Recipe_9204.img")
        //    };
        //    foreach (NXNode RecipeNode in RecipeNodes)
        //    {
        //        foreach (NXNode Recipe in RecipeNode)
        //        {
        //            WzRecipe AddRecipe = new WzRecipe();
        //            int RecipeId;
        //            if (!int.TryParse(Recipe.Name, out RecipeId) || DataBuffer.CraftRecipeBuffer.ContainsKey(RecipeId)) continue;

        //            int SkillId = (int)Math.Floor((double)RecipeId / 10000);
        //            SkillId *= 10000;
        //            AddRecipe.ReqSkill = SkillId;

        //            AddRecipe.IncFatigue = (byte)GetIntFromChild(Recipe, "incFatigability", 1);
        //            AddRecipe.ReqSkillLevel = (byte)GetIntFromChild(Recipe, "reqSkillLevel", 1);
        //            AddRecipe.IncProficiency = (byte)GetIntFromChild(Recipe, "incSkillProficiency", 1);

        //            if (Recipe.ContainsChild("recipe"))
        //            {
        //                NXNode ReqItemNode = Recipe["recipe"];
        //                foreach (NXNode ReqItem in ReqItemNode)
        //                {
        //                    WzRecipe.Item AddReqItem = new WzRecipe.Item();
        //                    AddReqItem.ItemId = GetIntFromChild(ReqItem, "item", 0);
        //                    if (AddReqItem.ItemId == 0) continue;
        //                    AddReqItem.Count = (short)GetIntFromChild(ReqItem, "count", 0);
        //                    AddReqItem.Chance = (byte)GetIntFromChild(ReqItem, "probWeight", 0);
        //                    AddRecipe.ReqItems.Add(AddReqItem);
        //                }
        //            }

        //            if (Recipe.ContainsChild("target"))
        //            {
        //                NXNode CreateItemNode = Recipe["target"];
        //                foreach (NXNode CeateItem in CreateItemNode)
        //                {
        //                    WzRecipe.Item AddCreateItem = new WzRecipe.Item();
        //                    AddCreateItem.ItemId = GetIntFromChild(CeateItem, "item", 0);
        //                    if (AddCreateItem.ItemId == 0) continue;
        //                    AddCreateItem.Count = (short)GetIntFromChild(CeateItem, "count", 0);
        //                    AddCreateItem.Chance = (byte)GetIntFromChild(CeateItem, "probWeight", 100);
        //                    AddRecipe.CreateItems.Add(AddCreateItem);
        //                }
        //            }

        //            DataBuffer.CraftRecipeBuffer.Add(RecipeId, AddRecipe);
        //            ret++;
        //        }
        //    }
        //    #endregion

        //    foreach (ManualResetEvent e in waitHandles)
        //        e.WaitOne();

        //    ret += DataBuffer.CharacterSkillBuffer.Count;

        //    File.Dispose();
        //    return ret;
        //}


        //public static int LoadStrings(String path)
        //{
        //    NXFile File = new NXFile(path);
        //    int count = 0;

        //    #region Equips
        //    NXNode EqpFolder = File.BaseNode.GetChild("Eqp.img").GetChild("Eqp");
        //    foreach (NXNode equipTypeNode in EqpFolder)
        //    {
        //        foreach (NXNode equipNode in equipTypeNode)
        //        {
        //            int id;
        //            if (int.TryParse(equipNode.Name, out id))
        //            {
        //                WzEquip wzEq = DataBuffer.GetEquipById(id);
        //                if (wzEq == null)
        //                    continue;
        //                wzEq.Name = GetStringFromChild(equipNode, "name");
        //                count++;
        //            }
        //        }
        //    }
        //    #endregion
        //    #region Items
        //    NXNode ConsumeImg = File.BaseNode.GetChild("Consume.img");
        //    foreach (NXNode consumeNode in ConsumeImg)
        //    {
        //        int id;
        //        if (int.TryParse(consumeNode.Name, out id))
        //        {
        //            WzItem wzItem = DataBuffer.GetItemById(id);
        //            if (wzItem == null)
        //                continue;
        //            wzItem.Name = GetStringFromChild(consumeNode, "name");
        //            count++;
        //        }
        //    }
        //    NXNode SetupImg = File.BaseNode.GetChild("Ins.img"); //chairs
        //    foreach (NXNode setupNode in SetupImg)
        //    {
        //        int id;
        //        if (int.TryParse(setupNode.Name, out id))
        //        {
        //            WzItem wzItem = DataBuffer.GetItemById(id);
        //            if (wzItem == null)
        //                continue;
        //            wzItem.Name = GetStringFromChild(setupNode, "name");
        //            count++;
        //        }
        //    }
        //    NXNode EtcFolder = File.BaseNode.GetChild("Etc.img").GetChild("Etc");
        //    foreach (NXNode etcNode in EtcFolder)
        //    {
        //        int id;
        //        if (int.TryParse(etcNode.Name, out id))
        //        {
        //            WzItem wzItem = DataBuffer.GetItemById(id);
        //            if (wzItem == null)
        //                continue;
        //            wzItem.Name = GetStringFromChild(etcNode, "name");
        //            count++;
        //        }
        //    }
        //    NXNode CashImg = File.BaseNode.GetChild("Cash.img");
        //    foreach (NXNode cashNode in CashImg)
        //    {
        //        int id;
        //        if (int.TryParse(cashNode.Name, out id))
        //        {
        //            WzItem wzItem = DataBuffer.GetItemById(id);
        //            if (wzItem == null)
        //                continue;
        //            wzItem.Name = GetStringFromChild(cashNode, "name");
        //            count++;
        //        }
        //    }
        //    NXNode PetImg = File.BaseNode.GetChild("Pet.img");
        //    foreach (NXNode petNode in PetImg)
        //    {
        //        int id;
        //        if (int.TryParse(petNode.Name, out id))
        //        {
        //            WzItem wzItem = DataBuffer.GetItemById(id);
        //            if (wzItem == null)
        //                continue;
        //            wzItem.Name = GetStringFromChild(petNode, "name");
        //            count++;
        //        }
        //    }
        //    #endregion
        //    #region Skills
        //    NXNode SkillImg = File.BaseNode.GetChild("Skill.img");
        //    foreach (NXNode skillNode in SkillImg)
        //    {
        //        bool skillBook = skillNode.Name.Length < 7;
        //        int id;
        //        if (int.TryParse(skillNode.Name, out id))
        //        {
        //            if (skillBook)
        //            {
        //                DataBuffer.JobNames.Add(id, GetStringFromChild(skillNode, "bookName"));
        //                count++;
        //            }
        //            else
        //            {
        //                WzCharacterSkill wzSkill = DataBuffer.GetCharacterSkillById(id);
        //                if (wzSkill == null)
        //                    continue;
        //                wzSkill.Name = GetStringFromChild(skillNode, "name");
        //                count++;
        //            }
        //        }
        //    }
        //    #endregion
        //    #region Maps
        //    NXNode MapImg = File.BaseNode.GetChild("Map.img");
        //    foreach (NXNode mapZone in MapImg)
        //    {
        //        foreach (NXNode mapNode in mapZone)
        //        {
        //            int id;
        //            if (int.TryParse(mapNode.Name, out id))
        //            {
        //                WzMap wzMap = DataBuffer.GetMapById(id);
        //                if (wzMap == null)
        //                    continue;
        //                wzMap.Name = GetStringFromChild(mapNode, "mapName");
        //                count++;
        //            }
        //        }
        //    }
        //    #endregion
        //    #region Mobs
        //    NXNode mobImg = File.BaseNode.GetChild("Mob.img");
        //    foreach (NXNode mobNode in mobImg)
        //    {
        //        int id;
        //        if (int.TryParse(mobNode.Name, out id))
        //        {
        //            WzMob wzMob = DataBuffer.GetMobById(id);
        //            if (wzMob == null)
        //                continue;
        //            wzMob.Name = GetStringFromChild(mobNode, "name");
        //            count++;
        //        }
        //    }
        //    #endregion
        //    #region Npcs
        //    NXNode npcImg = File.BaseNode.GetChild("Npc.img");
        //    foreach (NXNode npcNode in npcImg)
        //    {
        //        int id;
        //        if (int.TryParse(npcNode.Name, out id))
        //        {
        //            DataBuffer.NpcNames.Add(id, GetStringFromChild(npcNode, "name"));
        //            count++;
        //        }
        //    }
        //    #endregion

        //    return count;
        //}
    }
}
