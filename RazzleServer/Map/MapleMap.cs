using NLog;
using RazzleServer.Data.WZ;
using RazzleServer.Inventory;
using RazzleServer.Map.Monster;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Map
{
    public class MapleMap
    {
        public int MapID { get; set; }

        private int MaxMonsters;
        private AutoIncrement ObjectIDCounter = new AutoIncrement();

        private Dictionary<int, MapleCharacter> Characters = new Dictionary<int, MapleCharacter>();
        private List<WzMap.MobSpawn> MobSpawnPoints = new List<WzMap.MobSpawn>();

        private Dictionary<string, WzMap.Portal> Portals = new Dictionary<string, WzMap.Portal>();

        private Dictionary<int, WzMap.Reactor> Reactors = new Dictionary<int, WzMap.Reactor>();
        private Dictionary<int, int> DespawnedReactors = new Dictionary<int, int>();

        private Dictionary<int, WzMap.Npc> Npcs = new Dictionary<int, WzMap.Npc>();
        private Dictionary<int, MapleMonster> Mobs = new Dictionary<int, MapleMonster>();
        private Dictionary<int, MapleSummon> Summons = new Dictionary<int, MapleSummon>();
        //private Dictionary<int, MapleMapItem> MapItems = new Dictionary<int, MapleMapItem>();
        private Dictionary<int, StaticMapObject> StaticObjects = new Dictionary<int, StaticMapObject>(); //Magic door, Mists, etc

        private DateTime LastMobRespawnSpawn;
        private DateTime LastReactorRespawnSpawn;

        private object ReactorLock = new object();

        private WzMap WzInfo;

        public int ReturnMap => WzInfo.ReturnMap;

        private static Logger Log = LogManager.GetCurrentClassLogger();


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

        public bool MysticDoorLimit { get; internal set; }

        public void AddCharacter(MapleCharacter character)
        {
            lock (Characters)
            {
                if (Characters.ContainsKey(character.ID))
                    return;
                Characters.Add(character.ID, character);
                if (CharacterCount > 1)
                {
                    //ShowCharacters(character);
                    BroadcastPacket(MapleCharacter.SpawnPlayer(character), character);
                }
            }
            character.Map = this;
            character.MapID = MapID;

            //ShowNpcs(chr);
            //ShowMobs(chr);
            //ShowMapItems(chr);
            //ShowReactors(chr);
            //ShowStaticObjects(chr);
            //ShowSummons(chr); // Always do this before adding the chr's summons or they will be shown twice

            // foreach (MapleSummon summon in chr.GetSummons())
            // {
            //     summon.ObjectId = ObjectIdCounter.Get;
            //     summon.Position = chr.Position;
            //     AddSummon(summon, false);
            // }

            // if (chr.Party != null)
            // {
            //     var partyMembersOnMap = chr.Party.GetCharactersOnMap(this, chr.Id);
            //     if (partyMembersOnMap.Any())
            //     {
            //         PacketWriter hpPacket = MapleParty.Packets.UpdatePartyMemberHp(chr);
            //         foreach (MapleCharacter partyMember in partyMembersOnMap)
            //         {
            //             partyMember.Client.SendPacket(MapleParty.Packets.UpdateParty(chr.Party));
            //             if (chr == partyMember) continue;
            //             partyMember.Client.SendPacket(hpPacket);
            //             chr.Client.SendPacket(MapleParty.Packets.UpdatePartyMemberHp(partyMember));
            //         }
            //     }
            // }
        }

        #region Portals
        public WzMap.Portal GetDefaultSpawnPortal()
        {
            var spawnPortals = Portals.Values.Where(p => p.Type == WzMap.PortalType.Startpoint).OrderBy(p => p.ID);
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
            return portal != null ? portal.ID : (byte)0;
        }

        /// <summary>
        /// Gets this MapleMap's Portal that has the given Id
        /// </summary>
        /// <param name="Id">The Id of the requested Portal</param>
        /// <returns>Returns a WzMap.Portal object</returns>
        public WzMap.Portal GetStartpoint(byte Id)
        {
            return Portals.SingleOrDefault(x => x.Value.Type == WzMap.PortalType.Startpoint && x.Value.ID == Id).Value;
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
                // TODO - PORTAL ENGINE
                //PortalEngine.EnterScriptedPortal(portal, c.Account.Character);
            }
            else
            {
                Log.Error($"Unable to enter portal [{portalName}] in map [{c.Account.Character.MapID}]");
                c.Account.Character.SendBlueMessage($"[{portalName}] in map [{c.Account.Character.MapID}] is not scripted yet");
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
                            if (kvp.Value != null && kvp.Value.Client != null && (source == null || (!source.Hidden || kvp.Value.IsStaff))) //send if unknown source, source isn't hidden or receiving character is a GM
                                kvp.Value.Client.SendPacket(packet);
                        }
                    }
                    else
                    {
                        foreach (var kvp in Characters.Where(x => x.Value != source && (!!source.Hidden || x.Value.IsStaff)))
                        {
                            if (kvp.Value != null && kvp.Value.Client != null)
                                kvp.Value.Client.SendPacket(packet);
                        }
                    }
                }
            }
        }

        public void RemoveCharacter(int characterID)
        {
            lock (Characters)
            {
                MapleCharacter chr;
                if (Characters.TryGetValue(characterID, out chr))
                {
                    Characters.Remove(characterID);
                    BroadcastPacket(MapleCharacter.RemovePlayerFromMap(characterID));

                    //List<int> releasedMobs = RemoveMonsterControl(characterId);
                    //UpdateMonsterControl(releasedMobs);

                    // foreach (MapleSummon summon in chr.GetSummons())
                    // {
                    //     RemoveSummon(summon.ObjectId, false);
                    // }

                    // if (chr.Party != null)
                    // {
                    //     var partyMembers = chr.Party.GetCharactersOnMap(this, characterId);
                    //     foreach (MapleCharacter partyMember in partyMembers)
                    //     {
                    //         partyMember.Client.SendPacket(MapleParty.Packets.UpdateParty(chr.Party));
                    //     }
                    // }
                }
            }
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

        internal List<MapleCharacter> GetCharactersInRange(BoundingBox boundingBox, List<MapleCharacter> partyMembersOnSameMap)
        {
            throw new NotImplementedException();
        }

        internal List<MapleCharacter> GetCharactersInRange(BoundingBox boundingBox)
        {
            throw new NotImplementedException();
        }

        internal List<MapleMonster> GetMobsInRange(BoundingBox boundingBox)
        {
            throw new NotImplementedException();
        }

        internal void SpawnMist(int skillId, byte level, MapleCharacter source, BoundingBox boundingBox, Point sourcePos, int v1, bool v2)
        {
            throw new NotImplementedException();
        }

        internal Point GetDropPositionBelow(Point position1, Point position2)
        {
            throw new NotImplementedException();
        }

        internal void SpawnStaticObject(MysticDoor sourceDoor)
        {
            throw new NotImplementedException();
        }

        internal int GetNewObjectID()
        {
            throw new NotImplementedException();
        }

        internal void SpawnMapItem(MapleItem item, Point position, Point point, bool v, MapleDropType freeForAll, MapleCharacter owner)
        {
            throw new NotImplementedException();
        }

        internal void RemoveStaticObject(int objectId, bool v)
        {
            throw new NotImplementedException();
        }
    }
}