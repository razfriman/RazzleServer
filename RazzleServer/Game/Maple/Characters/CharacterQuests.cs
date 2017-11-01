﻿using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterQuests
    {
        public Character Parent { get; private set; }

        public Dictionary<ushort, Dictionary<int, short>> Started { get; private set; }
        public Dictionary<ushort, DateTime> Completed { get; private set; }

        public CharacterQuests(Character parent)
        {
            this.Parent = parent;

            this.Started = new Dictionary<ushort, Dictionary<int, short>>();
            this.Completed = new Dictionary<ushort, DateTime>();
        }

        public void Load()
        {
            foreach (Datum datum in new Datums("quests_started").Populate("CharacterID = {0}", this.Parent.ID))
            {
                if (!this.Started.ContainsKey((ushort)datum["QuestID"]))
                {
                    this.Started.Add((ushort)datum["QuestID"], new Dictionary<int, short>());
                }

                if (datum["MobID"] != null && datum["Killed"] != null)
                {
                    this.Started[(ushort)datum["QuestID"]].Add((int)datum["MobID"], ((short)datum["Killed"]));
                }
            }
        }

        public void Save()
        {
            foreach (KeyValuePair<ushort, Dictionary<int, short>> loopStarted in this.Started)
            {
                if (loopStarted.Value == null || loopStarted.Value.Count == 0)
                {
                    Datum datum = new Datum("quests_started");

                    datum["CharacterID"] = this.Parent.ID;
                    datum["QuestID"] = loopStarted.Key;

                    if (!Database.Exists("quests_started", "CharacterID = {0} && QuestID = {1}", this.Parent.ID, loopStarted.Key))
                    {
                        datum.Insert();
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, short> mobKills in loopStarted.Value)
                    {
                        Datum datum = new Datum("quests_started");

                        datum["CharacterID"] = this.Parent.ID;
                        datum["QuestID"] = loopStarted.Key;
                        datum["MobID"] = mobKills.Key;
                        datum["Killed"] = mobKills.Value;

                        if (Database.Exists("quests_started", "CharacterID = {0} && QuestID = {1} && MobID = {2}", this.Parent.ID, loopStarted.Key, mobKills.Key))
                        {
                            datum.Update("CharacterID = {0} && QuestID = {1} && MobID = {2}", this.Parent.ID, loopStarted.Key, mobKills.Key);
                        }
                        else
                        {
                            datum.Insert();
                        }
                    }
                }
            }

            foreach (KeyValuePair<ushort, DateTime> loopCompleted in this.Completed)
            {
                Datum datum = new Datum("quests_completed");

                datum["CharacterID"] = this.Parent.ID;
                datum["QuestID"] = loopCompleted.Key;
                datum["CompletionTime"] = loopCompleted.Value;

                if (Database.Exists("quests_completed", "CharacterID = {0} && QuestID = {1}", this.Parent.ID, loopCompleted.Key))
                {
                    datum.Update("CharacterID = {0} && QuestID = {1}", this.Parent.ID, loopCompleted.Key);
                }
                else
                {
                    datum.Insert();
                }
            }
        }

        public void Delete(ushort questID)
        {
            if (this.Started.ContainsKey(questID))
            {
                this.Started.Remove(questID);
            }

            if (Database.Exists("quests_started", "QuestID = {0}", questID))
            {
                Database.Delete("quests_started", "QuestID = {0}", questID);
            }
        }

        public void Delete()
        {

        }

        public void Handle(PacketReader iPacket)
        {
            var action = (QuestAction)iPacket.ReadByte();
            ushort questID = iPacket.ReadUShort();

            if (!DataProvider.Quests.Contains(questID))
            {
                return;
            }

            Quest quest = DataProvider.Quests[questID];

            int npcId;

            switch (action)
            {
                case QuestAction.RestoreLostItem: // TODO: Validate.
                    {
                        int quantity = iPacket.ReadInt();
                        int itemID = iPacket.ReadInt();

                        quantity -= this.Parent.Items.Available(itemID);

                        Item item = new Item(itemID, (short)quantity);

                        this.Parent.Items.Add(item);
                    }
                    break;

                case QuestAction.Start:
                    {
                        npcId = iPacket.ReadInt();

                        this.Start(quest, npcId);
                    }
                    break;

                case QuestAction.Complete:
                    {
                        npcId = iPacket.ReadInt();
                        iPacket.ReadInt(); // NOTE: Unknown
                        int selection = iPacket.Available >= 4 ? iPacket.ReadInt() : 0;

                        this.Complete(quest, selection);
                    }
                    break;

                case QuestAction.Forfeit:
                    {
                        this.Forfeit(quest.MapleID);
                    }
                    break;

                case QuestAction.ScriptStart:
                case QuestAction.ScriptEnd:
                    {
                        npcId = iPacket.ReadInt();

                        Npc npc = null;

                        foreach (Npc loopNpc in this.Parent.Map.Npcs)
                        {
                            if (loopNpc.MapleID == npcId)
                            {
                                npc = loopNpc;

                                break;
                            }
                        }

                        if (npc == null)
                        {
                            return;
                        }

                        this.Parent.Converse(npc, quest);
                    }
                    break;
            }
        }

        public void Start(Quest quest, int npcID)
        {
            this.Started.Add(quest.MapleID, new Dictionary<int, short>());

            foreach (KeyValuePair<int, short> requiredKills in quest.PostRequiredKills)
            {
                this.Started[quest.MapleID].Add(requiredKills.Key, 0);
            }

            this.Parent.Experience += quest.ExperienceReward[0];
            this.Parent.Fame += (short)quest.FameReward[0];
            this.Parent.Meso += quest.MesoReward[0] * Parent.Client.Server.World.MesoRate;

            // TODO: Skill and pet rewards.

            foreach (KeyValuePair<int, short> item in quest.PreItemRewards)
            {
                if (item.Value > 0)
                {
                    this.Parent.Items.Add(new Item(item.Key, item.Value)); // TODO: Quest items rewards are displayed in chat.
                }
                else if (item.Value < 0)
                {
                    this.Parent.Items.Remove(item.Key, Math.Abs(item.Value));
                }
            }

            this.Update(quest.MapleID, QuestStatus.InProgress);

            using (var oPacket = new PacketWriter(ServerOperationCode.QuestResult))
            {
                oPacket.WriteByte((byte)QuestResult.Complete);
                oPacket.WriteUShort(quest.MapleID);
                oPacket.WriteInt(npcID);
                oPacket.WriteInt(0);

                this.Parent.Client.Send(oPacket);
            }
        }

        public void Complete(Quest quest, int selection)
        {
            foreach (KeyValuePair<int, short> item in quest.PostRequiredItems)
            {
                this.Parent.Items.Remove(item.Key, item.Value);
            }

            this.Parent.Experience += quest.ExperienceReward[1];

            using (var oPacket = new PacketWriter(ServerOperationCode.Message))
            {
                oPacket.WriteByte((byte)MessageType.IncreaseEXP);
                oPacket.WriteBool(true);
                oPacket.WriteInt(quest.ExperienceReward[1]);
                oPacket.WriteBool(true);
                oPacket.WriteInt(0); // NOTE: Monster Book bonus.
                oPacket.WriteShort(0); // NOTE: Unknown.
                oPacket.WriteInt(0); // NOTE: Wedding bonus.
                oPacket.WriteByte(0); // NOTE: Party bonus.
                oPacket.WriteInt(0); // NOTE: Party bonus.
                oPacket.WriteInt(0); // NOTE: Equip bonus.
                oPacket.WriteInt(0); // NOTE: Internet Cafe bonus.
                oPacket.WriteInt(0); // NOTE: Rainbow Week bonus.
                oPacket.WriteByte(0); // NOTE: Unknown.

                this.Parent.Client.Send(oPacket);
            }

            this.Parent.Fame += (short)quest.FameReward[1];

            // TODO: Fame gain packet in chat.

            this.Parent.Meso += quest.MesoReward[1] * Parent.Client.Server.World.MesoRate;

            // TODO: Meso gain packet in chat.

            foreach (KeyValuePair<Skill, Job> skill in quest.PostSkillRewards)
            {
                if (this.Parent.Job == skill.Value)
                {
                    this.Parent.Skills.Add(skill.Key);

                    // TODO: Skill update packet.
                }
            }

            // TODO: Pet rewards.

            if (selection != -1) // NOTE: Selective reward.
            {
                //if (selection == -1) // NOTE: Randomized reward.
                //{
                //    KeyValuePair<int, short> item = quest.PostItemRewards.ElementAt(Constants.Random.Next(0, quest.PostItemRewards.Count));

                //    this.Parent.Items.Add(new Item(item.Key, item.Value));

                //    using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
                //    {
                //        oPacket
                //            oPacket.WriteByte((byte)UserEffect.Quest)
                //            oPacket.WriteByte(1)
                //            oPacket.WriteInt(item.Key)
                //            oPacket.WriteInt(item.Value);

                //        this.Parent.Client.Send(oPacket);
                //    }
                //}
                //else
                //{
                //    // TODO: Selective reward based on selection.
                //}
            }
            else
            {
                foreach (KeyValuePair<int, short> item in quest.PostItemRewards)
                {
                    if (item.Value > 0)
                    {
                        this.Parent.Items.Add(new Item(item.Key, item.Value));
                    }
                    else if (item.Value < 0)
                    {
                        this.Parent.Items.Remove(item.Key, Math.Abs(item.Value));
                    }

                    using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
                    {
                        oPacket.WriteByte((byte)UserEffect.Quest);
                        oPacket.WriteByte(1);
                        oPacket.WriteInt(item.Key);
                        oPacket.WriteInt(item.Value);

                        this.Parent.Client.Send(oPacket);
                    }
                }
            }

            this.Update(quest.MapleID, QuestStatus.Complete);

            this.Delete(quest.MapleID);

            this.Completed.Add(quest.MapleID, DateTime.UtcNow);

            this.Parent.ShowLocalUserEffect(UserEffect.QuestComplete);
            this.Parent.ShowRemoteUserEffect(UserEffect.QuestComplete, true);
        }

        public void Forfeit(ushort questID)
        {
            this.Delete(questID);

            this.Update(questID, QuestStatus.NotStarted);
        }

        private void Update(ushort questID, QuestStatus status, string progress = "")
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Message))
            {
                oPacket.WriteByte((byte)MessageType.QuestRecord);
                oPacket.WriteUShort(questID);
                oPacket.WriteByte((byte)status);

                if (status == QuestStatus.InProgress)
                {
                    oPacket.WriteString(progress);
                }
                else if (status == QuestStatus.Complete)
                {
                    oPacket.WriteDateTime(DateTime.Now);
                }

                this.Parent.Client.Send(oPacket);
            }
        }

        public bool CanComplete(ushort questID, bool onlyOnFinalKill = false)
        {
            Quest quest = DataProvider.Quests[questID];

            foreach (KeyValuePair<int, short> requiredItem in quest.PostRequiredItems)
            {
                if (!this.Parent.Items.Contains(requiredItem.Key, requiredItem.Value))
                {
                    return false;
                }
            }

            foreach (ushort requiredQuest in quest.PostRequiredQuests)
            {
                if (!this.Completed.ContainsKey(requiredQuest))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<int, short> requiredKill in quest.PostRequiredKills)
            {
                if (onlyOnFinalKill)
                {
                    if (this.Started[questID][requiredKill.Key] != requiredKill.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    if (this.Started[questID][requiredKill.Key] < requiredKill.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void NotifyComplete(ushort questID)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.QuestClear))
            {
                oPacket.WriteUShort(questID);

                this.Parent.Client.Send(oPacket);
            }
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteShort((short)this.Started.Count);

                foreach (KeyValuePair<ushort, Dictionary<int, short>> quest in this.Started)
                {
                    oPacket.WriteUShort(quest.Key);

                    string kills = string.Empty;

                    foreach (int kill in quest.Value.Values)
                    {
                        kills += kill.ToString().PadLeft(3, '\u0030');
                    }

                    oPacket.WriteString(kills);
                }

                oPacket.WriteShort((short)this.Completed.Count);

                foreach (KeyValuePair<ushort, DateTime> quest in this.Completed)
                {
                    oPacket.WriteUShort(quest.Key);
                        oPacket.WriteDateTime(quest.Value);
                }

                return oPacket.ToArray();
            }
        }
    }
}
