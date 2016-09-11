using System.Collections.Generic;
using RazzleServer.Player;
using System.Linq;
using RazzleServer.Util;
using RazzleServer.Packet;

namespace RazzleServer.Map
{
    public class MapleMap
    {
        public int MapID { get; set; }
        private AutoIncrement ObjectIdCounter = new AutoIncrement();

        public MapleMap ReturnMap { get; set; }

        private Dictionary<int, MapleCharacter> Characters = new Dictionary<int, MapleCharacter>();

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

        public MapleMapPortal GetDefaultSpawnPortal()
        {
            return null;
        }

        public MapleMapPortal GetPortal(string portalName)
        {
            return null;
        }

    }
}