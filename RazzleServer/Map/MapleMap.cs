using Microsoft.Extensions.Logging;
using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Handlers;
using RazzleServer.Inventory;
using RazzleServer.Map.Monster;
using RazzleServer.Packet;
using RazzleServer.Party;
using RazzleServer.Player;
using RazzleServer.Scripts;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Map
{
    public class MapleMap
    {
        public int MapID { get; set; }

        private int MaxMonsters;
        private AutoIncrement ObjectIDCounter = new AutoIncrement();
        private List<WzMap.MobSpawn> MobSpawnPoints = new List<WzMap.MobSpawn>();
        private Dictionary<int, MapleCharacter> Characters = new Dictionary<int, MapleCharacter>();
        private Dictionary<string, WzMap.Portal> Portals = new Dictionary<string, WzMap.Portal>();
        private Dictionary<int, WzMap.Reactor> Reactors = new Dictionary<int, WzMap.Reactor>();
        private Dictionary<int, int> DespawnedReactors = new Dictionary<int, int>();
        private Dictionary<int, WzMap.Npc> Npcs = new Dictionary<int, WzMap.Npc>();
        private Dictionary<int, MapleMonster> Mobs = new Dictionary<int, MapleMonster>();
        private Dictionary<int, MapleSummon> Summons = new Dictionary<int, MapleSummon>();
        private Dictionary<int, MapleMapItem> MapItems = new Dictionary<int, MapleMapItem>();
        private Dictionary<int, StaticMapObject> StaticObjects = new Dictionary<int, StaticMapObject>(); //Magic door, Mists, etc

        private DateTime LastMobRespawnSpawn;
        private DateTime LastReactorRespawnSpawn;

        private object ReactorLock = new object();

        private WzMap WzInfo;

        private static ILogger Log = LogManager.Log;


        public int ReturnMap => WzInfo.ReturnMap;

        public MapleMap(int mapId, WzMap wzMap, bool skipSpawn = false)
        {
            MapID = mapId;
            WzInfo = wzMap;
            if (!skipSpawn)
            {
                foreach (WzMap.Npc npc in wzMap.Npcs)
                {
                    int id = ObjectIDCounter.Get;
                    Npcs.Add(id, npc);
                }
                //foreach (var pair in Program.CustomNpcs)
                //{
                //    if (pair.Left != mapId) continue;
                //    int id = ObjectIDCounter.Get;
                //    Npcs.Add(id, pair.Right);
                //}
                Portals = wzMap.Portals;
                MobSpawnPoints = new List<WzMap.MobSpawn>(wzMap.MobSpawnPoints); //we need a copy for shuffling so mobs dont always spawn at the same place
                MobSpawnPoints.Shuffle();

                int spawnPointCount = MobSpawnPoints.Count;

                if (spawnPointCount >= 20)
                    MaxMonsters = (int)Math.Round((MobSpawnPoints.Count / wzMap.MobRate));
                else
                    MaxMonsters = (int)Math.Round((MobSpawnPoints.Count * wzMap.MobRate));

                if (MaxMonsters > 0)
                {
                    int count = 0;
                    int spawnPointCounter = 0;
                    while (count < MaxMonsters)
                    {
                        if (spawnPointCounter >= MobSpawnPoints.Count) //restart, for when mobrate > 1 and there should spawn more mobs than there are spawnpoints
                            spawnPointCounter = 0;

                        WzMap.MobSpawn mobSpawn = MobSpawnPoints[spawnPointCounter];
                        MapleMonster mob = ReSpawnMob(mobSpawn);
                        int id = ObjectIDCounter.Get;
                        mob.ObjectID = id;
                        Mobs.Add(id, mob);
                        count++;
                        spawnPointCounter++;
                    }
                }
                LastMobRespawnSpawn = DateTime.UtcNow;

                foreach (WzMap.Reactor reactor in WzInfo.Reactors)
                {
                    Reactors.Add(ObjectIDCounter.Get, reactor);
                }
            }
            else
            {
                foreach (WzMap.Portal portal in wzMap.Portals.Values.Where(x => x.Type == WzMap.PortalType.Startpoint).ToList())
                    Portals.Add(portal.Name, portal);
            }
        }

        public int GetNewObjectID()
        {
            return ObjectIDCounter.Get;
        }

        public void SpawnNpc(WzMap.Npc newNpc)
        {
            int id = ObjectIDCounter.Get;
            Npcs.Add(id, newNpc);

            BroadcastPacket(ShowNpc(id, newNpc));
        }

        public WzMap.Npc SpawnNpcOnGroundBelow(int npcId, Point position)
        {
            WzMap.FootHold fh = GetFootHoldBelow(new Point(position.X, position.Y - 1));
            if (fh == null) return null;
            Point npcPosition = GetPositionOnFootHold(fh, position.X);
            WzMap.Npc newNpc = new WzMap.Npc();
            newNpc.Id = npcId;
            newNpc.x = (short)npcPosition.X;
            newNpc.y = (short)npcPosition.Y;
            newNpc.Rx0 = 0;
            newNpc.Rx1 = 0;
            newNpc.Cy = (short)npcPosition.Y;
            newNpc.Fh = fh.Id;
            newNpc.F = true;
            newNpc.Hide = false;

            int id = ObjectIDCounter.Get;
            Npcs.Add(id, newNpc);

            BroadcastPacket(ShowNpc(id, newNpc));
            return newNpc;
        }

        public WzMap.FootHold GetRandomFoothold(Point maxPos, Point minPos)
        {
            List<WzMap.FootHold> validFootholds = WzInfo.FootHolds.Where(x => (x.Point1.X <= maxPos.X || x.Point1.X >= (maxPos.X * -1)) && (x.Point1.Y <= maxPos.Y || x.Point1.Y >= (maxPos.Y * -1)) && (x.Point1.X >= minPos.X || x.Point1.X <= (minPos.X * -1)) && (x.Point1.Y >= minPos.Y || x.Point1.Y <= (minPos.Y * -1))).ToList();
            return validFootholds[Functions.Random(0, validFootholds.Count - 1)];
        }

        public void UpdateMap(DateTime utcNow)
        {
            //expire items           
            lock (MapItems)
            {
                foreach (MapleMapItem mapItem in MapItems.Values.ToList())
                {
                    if (utcNow > mapItem.ExpireTime)
                    {
                        MapItems.Remove(mapItem.ObjectID);
                        BroadcastPacket(MapleMapItem.Packets.RemoveMapItem(mapItem.ObjectID, 0));
                    }
                    else if (mapItem.DropType != MapleDropType.FreeForAll && mapItem.DropType != MapleDropType.Boss && mapItem.DropType != MapleDropType.Unk && utcNow > mapItem.FFATime)
                        mapItem.DropType = MapleDropType.FreeForAll;
                }
            }
            lock (StaticObjects)
            {
                foreach (var objectKvp in StaticObjects.ToList())
                {
                    if (DateTime.UtcNow >= objectKvp.Value.Expiration)
                        RemoveStaticObject(objectKvp.Key, true);
                }
            }
            lock (Characters)
            {
                foreach (MapleCharacter chr in Characters.Values)
                {
                    chr.Update();
                }
            }
        }

        #region Characters
        public void AddCharacter(MapleCharacter chr)
        {
            lock (Characters)
            {
                if (Characters.ContainsKey(chr.ID))
                    return;
                Characters.Add(chr.ID, chr);
                if (CharacterCount > 1)
                {
                    ShowCharacters(chr);
                    BroadcastPacket(MapleCharacter.SpawnPlayer(chr), chr);
                }
            }
            chr.Map = this;
            chr.MapID = MapID;

            ShowNpcs(chr);
            ShowMobs(chr);
            ShowMapItems(chr);
            ShowReactors(chr);
            ShowStaticObjects(chr);
            ShowSummons(chr); // Always do this before adding the chr's summons or they will be shown twice

            foreach (MapleSummon summon in chr.GetSummons())
            {
                summon.ObjectID = ObjectIDCounter.Get;
                summon.Position = chr.Position;
                AddSummon(summon, false);
            }

            if (chr.Party != null)
            {
                var partyMembersOnMap = chr.Party.GetCharactersOnMap(this, chr.ID);
                if (partyMembersOnMap.Any())
                {
                    PacketWriter hpPacket = MapleParty.Packets.UpdatePartyMemberHp(chr);
                    foreach (MapleCharacter partyMember in partyMembersOnMap)
                    {
                        partyMember.Client.SendPacket(MapleParty.Packets.UpdateParty(chr.Party));
                        if (chr == partyMember) continue;
                        partyMember.Client.SendPacket(hpPacket);
                        chr.Client.SendPacket(MapleParty.Packets.UpdatePartyMemberHp(partyMember));
                    }
                }
            }
        }

        public void RemoveCharacter(int characterId)
        {
            lock (Characters)
            {
                MapleCharacter chr;
                if (Characters.TryGetValue(characterId, out chr))
                {
                    Characters.Remove(characterId);
                    BroadcastPacket(MapleCharacter.RemovePlayerFromMap(characterId));
                    List<int> releasedMobs = RemoveMonsterControl(characterId);
                    UpdateMonsterControl(releasedMobs);

                    foreach (MapleSummon summon in chr.GetSummons())
                    {
                        RemoveSummon(summon.ObjectID, false);
                    }
                    if (chr.Party != null)
                    {
                        var partyMembers = chr.Party.GetCharactersOnMap(this, characterId);
                        foreach (MapleCharacter partyMember in partyMembers)
                        {
                            partyMember.Client.SendPacket(MapleParty.Packets.UpdateParty(chr.Party));
                        }
                    }
                }
            }
        }

        public void ShowCharacters(MapleCharacter chr)
        {
            lock (Characters)
            {
                foreach (var characterKVP in Characters.Where(x => x.Key != chr.ID))
                {
                    chr.Client.SendPacket(MapleCharacter.SpawnPlayer(characterKVP.Value));
                    chr.Client.SendPacket(PlayerMovementHandler.CharacterMovePacket(chr.ID, new List<Movement.MapleMovementFragment>()));
                }
            }
        }

        public void HideCharacter(MapleCharacter chr)
        {
            lock (Characters)
            {
                foreach (var characterKvp in Characters.Where(x => x.Key != chr.ID && !x.Value.IsAdmin))
                {
                    characterKvp.Value.Client.SendPacket(MapleCharacter.RemovePlayerFromMap(chr.ID));
                }
            }
            List<int> releasedMobs = ReleaseAllMonsterControl(chr);
            UpdateMonsterControl(releasedMobs);
        }

        public void UnhideCharacter(MapleCharacter chr)
        {
            lock (Characters)
            {
                foreach (var characterKVP in Characters.Where(x => x.Key != chr.ID && !x.Value.IsAdmin))
                {
                    characterKVP.Value.Client.SendPacket(MapleCharacter.SpawnPlayer(chr));
                    chr.Client.SendPacket(PlayerMovementHandler.CharacterMovePacket(chr.ID, new List<Movement.MapleMovementFragment>()));
                }
            }
            UpdateMonsterControl();
        }
        #endregion

        #region Portals
        public WzMap.Portal GetDefaultSpawnPortal()
        {
            var spawnPortals = Portals.Values.Where(p => p.Type == WzMap.PortalType.Startpoint).OrderBy(p => p.Id);
            if (spawnPortals.Any())
            {
                return spawnPortals.FirstOrDefault();
            }
            return null;
        }

        public WzMap.Portal GetPortal(string name)
        {
            WzMap.Portal ret;
            if (Portals.TryGetValue(name, out ret))
            {
                return ret;
            }
            return null;
        }

        public WzMap.Portal GetClosestSpawnPoint(Point position)
        {
            var validSpawns = Portals.Where(p => p.Value.Type == WzMap.PortalType.Startpoint);
            if (validSpawns.Any())
            {
                var portalsOrderedByDistance = validSpawns.OrderBy(p => p.Value.Position.DistanceTo(position));
                return portalsOrderedByDistance.First().Value;
            }
            return null;
        }

        public WzMap.Portal TownPortal
        {
            get
            {
                return WzInfo.Portals.Values.Where(portal => portal.Type == WzMap.PortalType.TownportalPoint).FirstOrDefault();
            }
        }

        /// <summary>  
        /// Gets the nearest spawnpoint near the given Point
        /// </summary>
        /// <param name="position">Point which the spawnpoint should be closest to</param>
        /// <returns>A byte representing the spawnpoint's portal ID</returns>
        public byte GetClosestSpawnPointId(Point position)
        {
            WzMap.Portal portal = GetClosestSpawnPoint(position);
            return portal != null ? portal.Id : (byte)0;
        }

        /// <summary>
        /// Gets this MapleMap's Portal that has the given Id
        /// </summary>
        /// <param name="Id">The Id of the requested Portal</param>
        /// <returns>Returns a WzMap.Portal object</returns>
        public WzMap.Portal GetStartpoint(byte Id)
        {
            return Portals.SingleOrDefault(x => x.Value.Type == WzMap.PortalType.Startpoint && x.Value.Id == Id).Value;
        }

        /// <summary>
        /// Makes a character warp to the map linked to a portal
        /// </summary>
        /// <param name="c">The character's MapleClient</param>
        /// <param name="portalName">The name of the portal that the character is entering</param>
        public void EnterPortal(MapleClient c, string portalName)
        {
            WzMap.Portal portal = GetPortal(portalName);
            if (portal != null)
            {
                MapleMap toMap = ServerManager.GetChannelServer(c.Channel).GetMap(portal.ToMap);
                if (toMap != null)
                {
                    c.Account.Character.ChangeMap(toMap, portal.ToName);
                }
            }
            else
            {
                c.Account.Character.ChangeMap(ServerManager.GetChannelServer(c.Channel).GetMap(1000000));


                c.Account.Character.EnableActions();
                Log.LogError($"Cannot load portal [{portalName}] for map {MapID}]");
            }
        }
        /// <summary>
        /// Makes a character warp to the map linked to a scripted portal
        /// </summary>
        /// <param name="c">The character's MapleClient</param>
        /// <param name="portalName">The name of the portal that the character is entering</param>
        public void EnterPortalSpecial(MapleClient c, string portalName)
        {
            //not finished or done by any means
            WzMap.Portal portal = GetPortal(portalName);
            if (portal != null && !string.IsNullOrEmpty(portal.Script))
            {
                PortalEngine.EnterScriptedPortal(portal, c.Account.Character);
            }
            else
            {
                Log.LogError($"Unable to enter portal [{portalName}] in map [{c.Account.Character.MapID}]");
                c.Account.Character.SendBlueMessage($"[{portalName}] on [{c.Account.Character.MapID}] is not scripted yet");
            }
        }
        #endregion

        /// <summary>
        /// Broadcasts a Packet to every MapleCharacter on the map
        /// </summary>
        /// <param name="packet">The Packet to be broadcasted</param>
        /// <param name="source">The MapleCharacter the Packet is coming from</param>
        /// <param name="sendToSource">Whether the Packet should also be sent to the Source</param>
        public void BroadcastPacket(PacketWriter packet, MapleCharacter source = null, bool sendToSource = false)
        {
            lock (Characters)
            {
                if (Characters.Any())
                {
                    if (sendToSource == true || source == null)
                    {
                        foreach (var kvp in Characters)
                        {
                            if (kvp.Value != null && kvp.Value.Client != null && (source == null || (!source.Hidden || kvp.Value.IsAdmin))) //send if unknown source, source isn't hidden or receiving character is a GM
                                kvp.Value.Client.SendPacket(packet);
                        }
                    }
                    else
                    {
                        foreach (var kvp in Characters.Where(x => x.Value != source && (!!source.Hidden || x.Value.IsAdmin)))
                        {
                            if (kvp.Value != null && kvp.Value.Client != null)
                                kvp.Value.Client.SendPacket(packet);
                        }
                    }
                }
            }
        }

        public void BroadcastPartyPacket(PacketWriter packet, int partyId, MapleCharacter source = null, bool sendToSource = false)
        {
            lock (Characters)
            {
                int sourceId = source != null ? source.ID : 0;
                List<MapleCharacter> targets;
                if (source == null || sendToSource == true)
                    targets = Characters.Values.Where(chr => (source == null ? true : source.ID == chr.ID) || (chr.Party != null && chr.Party.ID == partyId)).ToList();
                else
                    targets = Characters.Values.Where(chr => chr.ID != source.ID && chr.Party != null && chr.Party.ID == partyId).ToList();
                foreach (MapleCharacter chr in targets)
                {
                    if (chr.Client != null)
                        chr.Client.SendPacket(packet);
                }
            }
        }

        #region Getters
        public MapleMonster GetMob(int objectId)
        {
            lock (Mobs)
            {
                MapleMonster ret;
                if (Mobs.TryGetValue(objectId, out ret))
                {
                    return ret;
                }
                return null;
            }
        }

        public WzMap.Npc GetNpc(int objectId)
        {
            WzMap.Npc ret;
            if (Npcs.TryGetValue(objectId, out ret))
            {
                return ret;
            }
            return null;
        }

        public bool HasNpc(int npcId)
        {
            foreach (var kvp in Npcs)
            {
                if (kvp.Value.Id == npcId)
                    return true;
            }
            return false;
        }

        public MapleMapItem GetMapItem(int objectId)
        {
            lock (MapItems)
            {
                MapleMapItem ret;
                if (MapItems.TryGetValue(objectId, out ret))
                {
                    return ret;
                }
            }
            return null;
        }

        public MapleCharacter GetCharacter(int characterId)
        {
            lock (Characters)
            {
                MapleCharacter chr;
                if (Characters.TryGetValue(characterId, out chr))
                    return chr;
                return null;
            }
        }

        public List<MapleCharacter> GetCharacters()
        {
            lock (Characters)
                return Characters.Values.ToList();
        }
        #endregion

        #region Mobs
        public void RemoveMob(int mobId)
        {
            lock (Mobs)
            {
                Mobs.Remove(mobId);
            }
        }

        public int CharacterCount
        {
            get
            {
                lock (Characters)
                {
                    return Characters.Count;
                }
            }
        }

        public void KillAllMobs(MapleCharacter killer)
        {
            lock (Mobs)
            {
                Dictionary<int, MapleMonster> mobList = Mobs.ToDictionary(x => x.Key, x => x.Value);
                foreach (var mobKvp in mobList)
                {
                    mobKvp.Value.Kill(killer);
                }
            }
        }

        public void SpawnMobOnGroundBelow(MapleMonster mob, Point position)
        {
            List<MapleMonster> mobs = new List<MapleMonster>() { { mob } };
            SpawnMobsOnGroundBelow(mobs, position);
        }

        public void SpawnMobsOnGroundBelow(List<MapleMonster> mobs, Point sourcePosition)
        {
            if (mobs == null || !mobs.Any())
                return;
            WzMap.FootHold fh = GetFootHoldBelow(new Point(sourcePosition.X, sourcePosition.Y - 1));
            if (fh == null)
                return;

            Point finalPosition = GetPositionOnFootHold(fh, sourcePosition.X);

            List<int> uncontrolled = new List<int>();
            lock (Mobs)
            {
                foreach (MapleMonster mob in mobs)
                {
                    mob.Stance = 2;
                    mob.Position = finalPosition;
                    mob.Fh = fh.Id;
                    int id = ObjectIDCounter.Get;
                    mob.ObjectID = id;
                    Mobs.Add(id, mob);
                    BroadcastPacket(MapleMonster.SpawnMob(id, mob, true));
                    uncontrolled.Add(id);
                }
            }
            UpdateMonsterControl(uncontrolled);
        }

        private void SpawnMob(MapleMonster mob)
        {
            if (mob != null)
            {
                List<int> uncontrolled = new List<int>();
                lock (Mobs)
                {
                    int id = ObjectIDCounter.Get;
                    mob.ObjectID = id;
                    Mobs.Add(id, mob);
                    BroadcastPacket(MapleMonster.SpawnMob(id, mob, true));
                    uncontrolled.Add(id);
                }
                UpdateMonsterControl(uncontrolled);
            }
        }

        private void SpawnMobs(List<MapleMonster> mobs)
        {
            if (mobs != null && mobs.Any())
            {
                List<int> uncontrolled = new List<int>();
                lock (Mobs)
                {
                    foreach (MapleMonster mob in mobs)
                    {
                        int id = ObjectIDCounter.Get;
                        mob.ObjectID = id;
                        Mobs.Add(id, mob);

                        BroadcastPacket(MapleMonster.SpawnMob(id, mob, true));

                        uncontrolled.Add(id);
                    }
                }
                UpdateMonsterControl(uncontrolled);
            }
        }

        //creates a new monster and returns it
        public MapleMonster ReSpawnMob(WzMap.MobSpawn spawnPoint)
        {
            MapleMonster mob = new MapleMonster(spawnPoint.wzMob, this);
            mob.Fh = spawnPoint.Fh;
            mob.Stance = 2;
            mob.Position = new Point(spawnPoint.Position.X, spawnPoint.Cy);

            return mob;
        }

        public void ShowMobs(MapleCharacter chr)
        {
            List<int> NoControl = new List<int>();
            lock (Mobs)
            {
                foreach (var kvp in Mobs)
                {
                    chr.Client.SendPacket(MapleMonster.SpawnMob(kvp.Key, kvp.Value));

                    MapleCharacter currentController;
                    if (!kvp.Value.Controller.TryGetTarget(out currentController) || currentController == null)
                    {
                        NoControl.Add(kvp.Key);
                    }
                    else
                    {
                        chr.Client.SendPacket(MapleMonster.RemoveMobControl(kvp.Key));
                    }
                }
            }
            UpdateMonsterControl(NoControl);
        }

        //Can be passed a list of monsterIds that do not have a controller yet, otherwise it finds them by itself (slower)
        public void UpdateMonsterControl(List<int> noControlList = null)
        {
            if (noControlList != null && noControlList.Count == 0)
                return;
            lock (Characters)
            {
                List<KeyValuePair<int, int>> currentControllerList = CountControllers();
                if (currentControllerList.Count == 0) //No viable controllers available
                    return;
                MapleCharacter newController = GetCharacter(currentControllerList.FirstOrDefault().Key);
                if (newController != null)
                {
                    if (noControlList != null)
                    {
                        lock (Mobs)
                        {
                            foreach (int objectId in noControlList)
                            {
                                MapleMonster mob = GetMob(objectId);
                                if (mob != null)
                                    mob.SetController(newController, objectId, false);
                            }
                        }
                    }
                    else
                    {
                        lock (Mobs)
                        {
                            foreach (var kvp in Mobs.Where(kvp => kvp.Value != null && (kvp.Value.GetController() == null || kvp.Value.GetController().Map != this))) //monster doesnt have controller or controller is not on this map
                            {
                                kvp.Value.SetController(newController, kvp.Key, false);
                            }
                        }
                    }
                }
            }
        }

        public List<int> ReleaseAllMonsterControl(MapleCharacter chr)
        {
            List<int> releasedMobObjectIds = new List<int>();
            lock (Characters)
            {
                lock (Mobs)
                {
                    foreach (var kvp in Mobs.Where(kvp => kvp.Value != null && kvp.Value.GetController().ID == chr.ID))
                    {
                        kvp.Value.ClearController();
                        releasedMobObjectIds.Add(kvp.Key);
                        chr.Client.SendPacket(MapleMonster.RemoveMobControl(kvp.Key));
                    }
                }
            }
            return releasedMobObjectIds;
        }

        public void CheckAndRespawnMobs()
        {
            int count = 0;
            lock (Mobs)
            {
                count = Mobs.Count;
            }
            if (MaxMonsters > 0 && count < MaxMonsters)
            {
                MobSpawnPoints.Shuffle();
                List<int> newMobs = new List<int>();
                int spawnPointCounter = 0;
                while (count < MaxMonsters)
                {
                    if (spawnPointCounter >= MobSpawnPoints.Count) //restart, for when mobrate > 1 and there should spawn more mobs than there are spawnpoints
                        spawnPointCounter = 0;
                    WzMap.MobSpawn mobSpawn = MobSpawnPoints[spawnPointCounter];
                    if (mobSpawn != null && mobSpawn.wzMob != null && mobSpawn.MobTime >= 0 && (DateTime.UtcNow - LastMobRespawnSpawn).TotalMilliseconds >= mobSpawn.MobTime)
                    {
                        MapleMonster mob = ReSpawnMob(mobSpawn);
                        int id = ObjectIDCounter.Get;
                        lock (Mobs)
                        {
                            mob.ObjectID = id;
                            Mobs.Add(id, mob);
                        }
                        newMobs.Add(id);
                        BroadcastPacket(MapleMonster.SpawnMob(id, mob, true));
                    }
                    count++;
                    spawnPointCounter++;
                }
                UpdateMonsterControl(newMobs);
            }
            LastMobRespawnSpawn = DateTime.UtcNow;

        }

        //Returns an ordered list of all non-hidden character IDs on the map and how many monsters they control (even if 0), ordered from lowest to highest amount
        private List<KeyValuePair<int, int>> CountControllers()
        {
            Dictionary<int, int> controllers = new Dictionary<int, int>();
            lock (Mobs)
            {
                foreach (var kvp in Mobs.Where(kvp => kvp.Value != null))
                {
                    MapleCharacter controller = kvp.Value.GetController();
                    if (controller != null)
                    {
                        if (controllers.ContainsKey(controller.ID))
                        {
                            controllers[controller.ID]++;
                        }
                        else
                        {
                            controllers.Add(controller.ID, 1);
                        }
                    }
                }
            }
            lock (Characters)
            {
                foreach (MapleCharacter chr in Characters.Values.Where(chr => !chr.Hidden && !controllers.ContainsKey(chr.ID)))
                {
                    controllers.Add(chr.ID, 0);
                }
            }
            return controllers.OrderBy(kvp => kvp.Value).ToList();
        }

        //returns a list of the monsters that were released from control
        private List<int> RemoveMonsterControl(int characterId)
        {
            List<int> releasedMobs = new List<int>();
            lock (Mobs)
            {
                foreach (var kvp in Mobs.Where(kvp => kvp.Value != null))
                {
                    MapleCharacter controller;
                    if (kvp.Value.Controller.TryGetTarget(out controller))
                    {
                        if (controller != null)
                        {
                            if (controller.ID == characterId)
                            {
                                kvp.Value.Controller.SetTarget(null);
                                releasedMobs.Add(kvp.Key);
                            }
                        }
                    }
                }
            }
            return releasedMobs;
        }

        #endregion

        #region Summons
        public void ShowSummons(MapleCharacter chr)
        {
            lock (Summons)
            {
                foreach (MapleSummon summon in Summons.Values)
                    chr.Client.SendPacket(summon.GetSpawnPacket(false));
            }
        }

        public void AddSummon(MapleSummon summon, bool animatedSpawn)
        {
            lock (Summons)
            {
                if (!Summons.ContainsKey(summon.ObjectID))
                    Summons.Add(summon.ObjectID, summon);
                BroadcastPacket(summon.GetSpawnPacket(animatedSpawn));
            }
        }

        public void RemoveSummon(int objectId, bool animatedRemoval)
        {
            lock (Summons)
            {
                MapleSummon summon;
                if (Summons.TryGetValue(objectId, out summon))
                {
                    Summons.Remove(objectId);
                    BroadcastPacket(summon.RemovePacket(animatedRemoval));
                }
            }
        }

        public MapleSummon GetSummon(int objectId)
        {
            lock (Summons)
            {
                MapleSummon summon;
                if (Summons.TryGetValue(objectId, out summon))
                    return summon;
                return null;
            }
        }
        #endregion

        #region Static Objects
        public void ShowStaticObjects(MapleCharacter chr)
        {
            lock (StaticObjects)
            {
                foreach (StaticMapObject obj in StaticObjects.Values)
                {

                    if (obj.IsPartyObject)
                    {
                        if (obj.Owner == chr || (chr.Party != null && chr.Party.ID == obj.PartyId))
                        {
                            chr.Client.SendPacket(obj.GetSpawnPacket(false));
                        }
                    }
                    else
                        chr.Client.SendPacket(obj.GetSpawnPacket(false));
                }
            }
        }

        public void AddStaticObject(StaticMapObject obj, int objectId, bool animatedSpawn)
        {
            lock (StaticObjects)
            {
                obj.ObjectID = objectId;
                StaticObjects.Add(objectId, obj);
                if (obj.IsPartyObject)
                    BroadcastPartyPacket(obj.GetSpawnPacket(animatedSpawn), obj.PartyId);
                else
                    BroadcastPacket(obj.GetSpawnPacket(animatedSpawn));
            }
        }

        public void RemoveStaticObject(int objectId, bool animatedDestroy)
        {
            lock (StaticObjects)
            {
                StaticMapObject obj;
                if (StaticObjects.TryGetValue(objectId, out obj))
                {
                    StaticObjects.Remove(objectId);
                    if (obj.IsPartyObject)
                    {
                        BroadcastPartyPacket(obj.GetDestroyPacket(true), obj.PartyId, obj.Owner, true);
                    }
                    else
                    {
                        BroadcastPacket(obj.GetDestroyPacket(animatedDestroy));
                    }
                    obj.Dispose();
                }
            }
        }

        public MapleMist SpawnMist(int skillId, byte skillLevel, MapleCharacter owner, BoundingBox boundingBox, Point position, int durationMS, bool partyObject)
        {
            int objectId = ObjectIDCounter.Get;
            MapleMist mist = new MapleMist(skillId, skillLevel, objectId, owner, boundingBox, position, durationMS, partyObject);
            AddStaticObject(mist, objectId, true);
            return mist;
        }

        public void SpawnStaticObject(StaticMapObject obj)
        {
            int oid = ObjectIDCounter.Get;
            AddStaticObject(obj, oid, true);
        }

        public SpecialPortal GetDoor(int ownerId)
        {
            lock (StaticObjects)
            {
                foreach (SpecialPortal obj in StaticObjects.Values)
                {
                    if (obj.Owner != null && ownerId == obj.Owner.ID)
                        return obj;
                }
            }
            return null;
        }
        #endregion

        #region Items
        public void ShowMapItems(MapleCharacter chr)
        {
            lock (MapItems)
            {
                foreach (var kvp in MapItems)
                {
                    chr.Client.SendPacket(MapleMapItem.Packets.SpawnMapItem(kvp.Value, kvp.Value.Position, 2));
                }
            }
        }

        //if meso > 0 it will include a meso drop as well
        public void SpawnMapItemsFromMonster(MapleMonster mob, Point sourcePosition, MapleCharacter dropOwner)
        {
            int count = 1;
            MapleDropType dropType = mob.WzInfo.ExplosiveReward ? MapleDropType.Boss : mob.WzInfo.FFALoot ? MapleDropType.FreeForAll : MapleDropType.Player; //TODO: check for part 
            List<MobDrop> drops = DataBuffer.GetMobDropsById(mob.WzInfo.MobId);
            if (drops.Count > 0) //global drops only for mobs that have drops
            {
                drops = new List<MobDrop>(drops);
                drops.AddRange(DataBuffer.GlobalDropBuffer);
            }
            foreach (MobDrop mobDrop in drops)
            {
                int chance = (int)(mobDrop.DropChance * (dropOwner.Stats.DropR / 100.0) * ServerConfig.Instance.DropRate);
                if (Functions.Random(0, 999999) <= chance)
                {
                    if (mobDrop.QuestID > 0)
                    {
                        bool shouldDrop = false;
                        if (dropOwner.Party != null)
                        {
                            List<MapleCharacter> partyCharacters = dropOwner.Party.GetCharactersOnMap(dropOwner.Map);
                            foreach (MapleCharacter chr in partyCharacters)
                            {
                                if (chr.HasQuestInProgress(mobDrop.QuestID))
                                {
                                    shouldDrop = true;
                                    break;
                                }
                            }
                        }
                        else if (dropOwner.HasQuestInProgress(mobDrop.QuestID))
                            shouldDrop = true;

                        if (!shouldDrop)
                            continue;
                    }

                    MapleMapItem mapItem = null;
                    Point targetPosition;
                    if (count % 2 == 0)
                        targetPosition = new Point(sourcePosition.X + (count / 2) * (dropType == MapleDropType.Boss ? 40 : 25), sourcePosition.Y - 50);
                    else
                        targetPosition = new Point(sourcePosition.X - (count / 2) * (dropType == MapleDropType.Boss ? 40 : 25), sourcePosition.Y - 50);
                    targetPosition = GetDropPositionBelow(targetPosition, sourcePosition);

                    int id = ObjectIDCounter.Get;

                    if (mobDrop.ItemID == 0) //meso
                    {
                        int amount = Functions.Random(mobDrop.MinQuantity, mobDrop.MaxQuantity);
                        if (amount > 0)
                        {
                            amount = (int)(amount * (dropOwner.Stats.MesoR / 100.0) * ServerConfig.Instance.MesoRate);
                            mapItem = new MapleMapItem(id, null, targetPosition, dropOwner.ID, dropType, false, amount);
                        }
                    }
                    else
                    {
                        if (ItemConstants.GetInventoryType(mobDrop.ItemID) == MapleInventoryType.Equip)
                        {
                            WzEquip equipInfo = DataBuffer.GetEquipById(mobDrop.ItemID);
                            if (equipInfo != null)
                            {
                                MapleEquip equip = (MapleEquip)MapleItemCreator.CreateItem(equipInfo.ItemId, string.Format("Mobdrop from {0} on map {1} with dropowner {2}", mob.WzInfo.MobId, WzInfo.MapId, dropOwner.Name));
                                equip.SetDefaultStats(equipInfo, true);
                                //TODO: add potential
                                mapItem = new MapleMapItem(id, equip, targetPosition, dropOwner.ID, dropType, false);
                            }
                        }
                        else
                        {
                            WzItem itemInfo = DataBuffer.GetItemById(mobDrop.ItemID);
                            if (itemInfo != null)
                            {
                                short quantity;
                                if (mobDrop.MaxQuantity > mobDrop.MinQuantity)
                                    quantity = (short)Functions.Random(mobDrop.MinQuantity, mobDrop.MaxQuantity);
                                else
                                    quantity = (short)mobDrop.MinQuantity;
                                if (quantity > 0)
                                {
                                    MapleItem item = MapleItemCreator.CreateItem(itemInfo.ItemId, string.Format("Mobdrop from {0} on map {1} with dropowner {2}", mob.WzInfo.MobId, WzInfo.MapId, dropOwner.Name));
                                    mapItem = new MapleMapItem(id, item, targetPosition, dropOwner.ID, dropType, false);
                                }
                            }
                        }
                    }
                    if (mapItem != null)
                    {
                        MapItems.Add(mapItem.ObjectID, mapItem);
                        BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 0, mob.ObjectID));
                        BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 1, mob.ObjectID));
                        count++;
                    }
                }
            }
        }

        public void SpawnMapItem(MapleItem item, Point sourcePosition, Point targetPosition, bool playerDrop, MapleDropType dropType, MapleCharacter dropOwner = null)
        {
            int id = ObjectIDCounter.Get;
            if (dropOwner == null)
                dropType = MapleDropType.FreeForAll;
            int ownerId = dropOwner?.ID ?? -1;
            MapleMapItem mapItem = new MapleMapItem(id, item, targetPosition, ownerId, dropType, playerDrop);
            lock (MapItems)
            {
                MapItems.Add(mapItem.ObjectID, mapItem);
            }
            BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 0));
            BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 1));
        }

        public void SpawnMesoMapItem(int amount, Point sourcePosition, Point targetPosition, bool playerDrop, MapleDropType dropType, MapleCharacter dropOwner = null)
        {
            if (amount <= 0)
                return;
            int id = ObjectIDCounter.Get;
            int ownerId = dropOwner?.ID ?? 0;
            MapleMapItem mapItem = new MapleMapItem(id, null, targetPosition, ownerId, dropType, playerDrop, amount);
            lock (MapItems)
            {
                MapItems.Add(mapItem.ObjectID, mapItem);
            }
            BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 0));
            BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(mapItem, sourcePosition, 1));
        }

        public bool HandlePlayerItemPickup(MapleClient c, int mapItemObjectId)
        {
            lock (MapItems)
            {
                MapleMapItem mapItem = GetMapItem(mapItemObjectId);
                if (mapItem == null) return false;
                //TODO: check for quest
                if (mapItem.PlayerDrop || mapItem.DropType == MapleDropType.FreeForAll || mapItem.DropType == MapleDropType.Boss || mapItem.DropType == MapleDropType.Unk || (mapItem.DropType == 0 && mapItem.OwnerID == c.Account.Character.ID))  //TODO: party, dropType == 1 and ownerId is in player's party
                {
                    MapleCharacter chr = c.Account.Character;
                    bool remove = false;
                    if (mapItem.Meso > 0)
                    {
                        if (chr.Inventory.Mesos < int.MaxValue)
                        {
                            if (mapItem.PlayerDrop)
                            {
                                //todo: subtract tax if from other player
                            }
                            c.Account.Character.Inventory.GainMesos(mapItem.Meso);
                            remove = true;
                        }
                    }
                    else if (mapItem.Item != null)
                    {
                        if (mapItem.Item.ItemId == 2431835) //Mystic Stolen Potion
                        {
                            remove = true;
                            chr.AddMP((int)(chr.Stats.MaxMp * 0.2));
                        }
                        else
                        {
                            if (c.Account.Character.Inventory.AddItem(mapItem.Item, mapItem.Item.InventoryType))
                                remove = true;
                        }
                    }
                    if (remove)
                    {
                        MapItems.Remove(mapItemObjectId);
                        BroadcastPacket(MapleMapItem.Packets.RemoveMapItem(mapItemObjectId, 2, c.Account.Character.ID));
                        if (mapItem.Item != null)
                            c.SendPacket(MapleInventory.Packets.ShowItemGain(mapItem.Item.ItemId, mapItem.Item.Quantity, true));
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveAllMapItems()
        {
            lock (MapItems)
            {
                var mapItems = MapItems.ToList();
                foreach (var mapItemKVP in mapItems)
                {
                    if (MapItems.Remove(mapItemKVP.Key))
                        BroadcastPacket(MapleMapItem.Packets.RemoveMapItem(mapItemKVP.Key, 0));
                }
            }
        }
        #endregion

        #region Reactors
        public void CheckAndRespawnReactors()
        {
            if (LastReactorRespawnSpawn == DateTime.MinValue)
                LastReactorRespawnSpawn = DateTime.UtcNow;
            else
            {
                int SecondsDiff = (int)Math.Ceiling((DateTime.UtcNow - LastReactorRespawnSpawn).TotalSeconds);
                LastReactorRespawnSpawn = DateTime.UtcNow;
                if (SecondsDiff < 0) return; //Wierd bug
                lock (ReactorLock)
                {
                    if (DespawnedReactors.Any())
                    {
                        List<int> Temp = new List<int>();
                        Dictionary<int, int> Copy = new Dictionary<int, int>();
                        foreach (KeyValuePair<int, int> Reactor in DespawnedReactors)
                            Copy.Add(Reactor.Key, Reactor.Value - SecondsDiff);
                        DespawnedReactors = Copy;

                        foreach (KeyValuePair<int, int> Reactor in DespawnedReactors.Where(x => x.Value <= 0))
                            Temp.Add(Reactor.Key);

                        foreach (int ObjectId in Temp)
                        {
                            DespawnedReactors.Remove(ObjectId);
                            KeyValuePair<int, WzMap.Reactor> toSpawnReactor = Reactors.SingleOrDefault(x => x.Key == ObjectId);
                            BroadcastPacket(SpawnReactor(toSpawnReactor.Key, toSpawnReactor.Value));
                        }
                    }
                }
            }
        }

        public void ShowReactors(MapleCharacter chr)
        {
            foreach (KeyValuePair<int, WzMap.Reactor> ReactorItem in Reactors)
            {
                if (!DespawnedReactors.ContainsKey(ReactorItem.Key))
                    chr.Client.SendPacket(SpawnReactor(ReactorItem.Key, ReactorItem.Value));
            }
        }

        public void DestroyReactor(int objectId)
        {
            lock (ReactorLock)
            {
                if (!Reactors.ContainsKey(objectId) || DespawnedReactors.ContainsKey(objectId)) return;
                DespawnedReactors.Add(objectId, Reactors[objectId].ReactorTime);
                DestroyReactor(objectId);
                //Todo: drop items
            }
        }

        public Point GetReactorPos(int objectId)
        {
            if (!Reactors.ContainsKey(objectId) || DespawnedReactors.ContainsKey(objectId)) return new Point(0, 0);
            return Reactors[objectId].Position;
        }

        public static PacketWriter SpawnReactor(int objectId, WzMap.Reactor Reactor)
        {
            //[F4 65 03 00] [41 0D 03 00] [00] [[CE 00] [FD 01]] [00 00 00]

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.REACTOR_SPAWN);
            pw.WriteInt(objectId);
            pw.WriteInt(Reactor.Id);
            pw.WriteByte(Reactor.State);
            pw.WritePoint(Reactor.Position);
            pw.WriteZeroBytes(3); //Unk
            return pw;
        }

        public static PacketWriter DestroyReactor(int objectId, WzMap.Reactor Reactor)
        {
            //F4 65 03 00 04 CE 00 FD 01

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.REACTOR_SPAWN);
            pw.WriteInt(objectId);
            pw.WriteByte(4); //Unk
            pw.WritePoint(Reactor.Position);
            return pw;
        }
        #endregion

        #region NPCs
        public void ShowNpcs(MapleCharacter Character)
        {
            foreach (var Npc in Npcs)
            {
                Character.Client.SendPacket(ShowNpc(Npc.Key, Npc.Value));
            }
        }

        public static PacketWriter ShowNpc(int objectId, WzMap.Npc Npc)
        {

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SPAWN_NPC);
            pw.WriteInt(objectId);
            pw.WriteInt(Npc.Id);
            pw.WriteShort(Npc.x);
            pw.WriteShort(Npc.Cy);
            pw.WriteBool(Npc.F);
            pw.WriteShort(Npc.Fh);
            pw.WriteShort(Npc.Rx0);
            pw.WriteShort(Npc.Rx1);
            pw.WriteBool(!Npc.Hide);
            return pw;
        }
        #endregion

        #region Position calculation
        public WzMap.FootHold GetFootHoldBelow(Point position)
        {
            var validXFootHolds = WzInfo.FootHolds.Where(fh => !fh.IsWall && position.X >= fh.Point1.X && position.X <= fh.Point2.X && (fh.Point1.Y >= position.Y || fh.Point2.Y >= position.Y));
            if (validXFootHolds.Any())
            {
                foreach (WzMap.FootHold fh in validXFootHolds.OrderBy(fh => (fh.Point1.Y < fh.Point2.Y ? fh.Point1.Y : fh.Point2.Y)))
                {
                    if (fh.Point1.Y != fh.Point2.Y) //diagonal foothold
                    {
                        int width = Math.Abs(fh.Point2.X - fh.Point1.X);
                        int height = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                        double xy = (double)height / width;

                        int distFromPoint1 = position.X - fh.Point1.X; //doesnt matter if abs or not as long as you use point1's Y too

                        int addedY = (int)(distFromPoint1 * xy);

                        int y = fh.Point1.Y + addedY; //Foothold's Y value on position.X

                        if (y >= position.Y)
                            return fh;
                    }
                    else
                    {
                        return fh;
                    }
                }
            }
            return null;
        }

        public Point GetPositionOnFootHold(WzMap.FootHold fh, int x)
        {
            if (fh.Point1.Y != fh.Point2.Y) //diagonal foothold
            {
                int width = Math.Abs(fh.Point2.X - fh.Point1.X);
                int height = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                if (fh.Point1.Y > fh.Point2.Y)
                    height *= -1; //left-bottom to right-top
                double xy = (double)height / width;

                int distFromPoint1 = x - fh.Point1.X;

                int addedY = (int)(distFromPoint1 * xy);

                int y = fh.Point1.Y + addedY;
                return new Point(x, y);
            }
            else
                return new Point(x, fh.Point1.Y);

        }

        public Point GetDropPositionBelow(Point sourcePosition, Point fallBack)
        {
            int dropX = sourcePosition.X;
            int dropY = sourcePosition.Y;
            if (dropX > WzInfo.RightBorder)
                dropX = WzInfo.RightBorder;
            else if (dropX < WzInfo.LeftBorder)
                dropX = WzInfo.LeftBorder;
            WzMap.FootHold fh = GetFootHoldBelow(new Point(dropX, dropY));
            if (fh == null) //couldnt find a foothold below... 
            {
                fh = GetFootHoldBelow(new Point(dropX, WzInfo.TopBorder)); //finding from the top...
                if (fh == null) //still nothing found... use fallback
                    return fallBack;
            }
            return GetPositionOnFootHold(fh, sourcePosition.X);
        }

        public List<MapleCharacter> GetCharactersInRange(BoundingBox boundingBox, List<MapleCharacter> possibleTargets = null)
        {
            List<MapleCharacter> ret = new List<MapleCharacter>();
            if (possibleTargets == null)
            {
                lock (Characters)
                {
                    possibleTargets = Characters.Values.ToList();
                }
            }
            foreach (MapleCharacter chr in possibleTargets)
            {
                if (boundingBox.Contains(chr.Position))
                    ret.Add(chr);
            }
            return ret;
        }

        public List<MapleMonster> GetMobsInRange(BoundingBox boundingBox)
        {
            List<MapleMonster> ret = new List<MapleMonster>();
            lock (Mobs)
            {
                foreach (MapleMonster mob in Mobs.Values)
                {
                    if (boundingBox.Contains(mob.Position))
                        ret.Add(mob);
                }
            }
            return ret;
        }
        #endregion

        #region FieldLimits
        public bool JumpLimit => (WzInfo.Limit & WzMap.FieldLimit.Jump) > 0;
        public bool MovementSkillLimit => (WzInfo.Limit & WzMap.FieldLimit.MovementSkill) > 0;
        public bool SummonBagLimit => (WzInfo.Limit & WzMap.FieldLimit.SummonBag) > 0;
        public bool MysticDoorLimit => (WzInfo.Limit & WzMap.FieldLimit.MysticDoor) > 0;
        public bool ChangeChannelLimit => (WzInfo.Limit & WzMap.FieldLimit.ChangeChannel) > 0;
        public bool PortalScrollLimit => (WzInfo.Limit & WzMap.FieldLimit.PortalScroll) > 0;
        public bool TeleportItemLimit => (WzInfo.Limit & WzMap.FieldLimit.TeleportItem) > 0;
        public bool MiniGameLimit => (WzInfo.Limit & WzMap.FieldLimit.MiniGame) > 0;
        public bool SpecificPortalScrollLimit => (WzInfo.Limit & WzMap.FieldLimit.SpecificPortalScroll) > 0;
        public bool MountLimit => (WzInfo.Limit & WzMap.FieldLimit.Mount) > 0;
        public bool PotionLimit => (WzInfo.Limit & WzMap.FieldLimit.Potion) > 0;
        public bool PartyLeaderChangeLimit => (WzInfo.Limit & WzMap.FieldLimit.PartyLeaderChange) > 0;
        public bool NoMobCapacityLimit => (WzInfo.Limit & WzMap.FieldLimit.NoMobCapacity) > 0;
        public bool WeddingInvitationLimit => (WzInfo.Limit & WzMap.FieldLimit.WeddingInvitation) > 0;
        public bool CashShopWeatherItemLimit => (WzInfo.Limit & WzMap.FieldLimit.CashShopWeatherItem) > 0;
        public bool PetLimit => (WzInfo.Limit & WzMap.FieldLimit.Pet) > 0;
        public bool MacroLimit => (WzInfo.Limit & WzMap.FieldLimit.AntiMacro) > 0;
        public bool FallDownLimit => (WzInfo.Limit & WzMap.FieldLimit.FallDown) > 0;
        public bool SummonNpcLimit => (WzInfo.Limit & WzMap.FieldLimit.SummonNpc) > 0;
        public bool NoExpDecreaseLimit => (WzInfo.Limit & WzMap.FieldLimit.NoExpDecrease) > 0;
        public bool NoDamageOnFallingLimit => (WzInfo.Limit & WzMap.FieldLimit.NoDamageOnFalling) > 0;
        public bool OpenParcelLimit => (WzInfo.Limit & WzMap.FieldLimit.OpenParcel) > 0;
        public bool DropItemLimit => (WzInfo.Limit & WzMap.FieldLimit.DropItem) > 0;

        #endregion
    }
}