﻿using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterQuests
    {
        public Character Parent { get; }

        public Dictionary<int, Dictionary<int, short>> Started { get; }
        public Dictionary<int, DateTime> Completed { get; }

        public CharacterQuests(Character parent)
        {
            Parent = parent;

            Started = new Dictionary<int, Dictionary<int, short>>();
            Completed = new Dictionary<int, DateTime>();
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("quests_started").Populate("CharacterId = {0}", this.Parent.Id))
            //{
            //    if (!this.Started.ContainsKey((int)datum["QuestId"]))
            //    {
            //        this.Started.Add((int)datum["QuestId"], new Dictionary<int, short>());
            //    }

            //    if (datum["MobId"] != null && datum["Killed"] != null)
            //    {
            //        this.Started[(int)datum["QuestId"]].Add((int)datum["MobId"], ((short)datum["Killed"]));
            //    }
            //}
        }

        public void Save()
        {
            //foreach (KeyValuePair<int, Dictionary<int, short>> loopStarted in this.Started)
            //{
            //    if (loopStarted.Value == null || loopStarted.Value.Count == 0)
            //    {
            //        Datum datum = new Datum("quests_started");

            //        datum["CharacterId"] = this.Parent.Id;
            //        datum["QuestId"] = loopStarted.Key;

            //        if (!Database.Exists("quests_started", "CharacterId = {0} && QuestId = {1}", this.Parent.Id, loopStarted.Key))
            //        {
            //            datum.Insert();
            //        }
            //    }
            //    else
            //    {
            //        foreach (KeyValuePair<int, short> mobKills in loopStarted.Value)
            //        {
            //            Datum datum = new Datum("quests_started");

            //            datum["CharacterId"] = this.Parent.Id;
            //            datum["QuestId"] = loopStarted.Key;
            //            datum["MobId"] = mobKills.Key;
            //            datum["Killed"] = mobKills.Value;

            //            if (Database.Exists("quests_started", "CharacterId = {0} && QuestId = {1} && MobId = {2}", this.Parent.Id, loopStarted.Key, mobKills.Key))
            //            {
            //                datum.Update("CharacterId = {0} && QuestId = {1} && MobId = {2}", this.Parent.Id, loopStarted.Key, mobKills.Key);
            //            }
            //            else
            //            {
            //                datum.Insert();
            //            }
            //        }
            //    }
            //}

            //foreach (KeyValuePair<int, DateTime> loopCompleted in this.Completed)
            //{
            //    Datum datum = new Datum("quests_completed");

            //    datum["CharacterId"] = this.Parent.Id;
            //    datum["QuestId"] = loopCompleted.Key;
            //    datum["CompletionTime"] = loopCompleted.Value;

            //    if (Database.Exists("quests_completed", "CharacterId = {0} && QuestId = {1}", this.Parent.Id, loopCompleted.Key))
            //    {
            //        datum.Update("CharacterId = {0} && QuestId = {1}", this.Parent.Id, loopCompleted.Key);
            //    }
            //    else
            //    {
            //        datum.Insert();
            //    }
            //}
        }

        public void Delete(int questId)
        {
            if (Started.ContainsKey(questId))
            {
                Started.Remove(questId);
            }

            //if (Database.Exists("quests_started", "QuestId = {0}", questId))
            //{
            //    Database.Delete("quests_started", "QuestId = {0}", questId);
            //}
        }

        public void Delete()
        {
        }

        public void Handle(PacketReader iPacket)
        {
            var action = (QuestAction)iPacket.ReadByte();
            var questId = iPacket.ReadInt();

            if (!DataProvider.Quests.Data.ContainsKey(questId))
            {
                Parent.LogCheatWarning(CheatType.InvalidQuest);
                return;
            }

            var quest = DataProvider.Quests.Data[questId];

            int npcId;

            switch (action)
            {
                case QuestAction.RestoreLostItem: // TODO: Validate.
                {
                    var quantity = iPacket.ReadInt();
                    var itemId = iPacket.ReadInt();

                    quantity -= Parent.Items.Available(itemId);

                    var item = new Item(itemId, (short)quantity);

                    Parent.Items.Add(item);
                }
                    break;

                case QuestAction.Start:
                {
                    npcId = iPacket.ReadInt();

                    Start(quest, npcId);
                }
                    break;

                case QuestAction.Complete:
                {
                    npcId = iPacket.ReadInt();
                    iPacket.ReadInt(); // NOTE: Unknown
                    var selection = iPacket.Available >= 4 ? iPacket.ReadInt() : 0;

                    Complete(quest, selection);
                }
                    break;

                case QuestAction.Forfeit:
                {
                    Forfeit(quest.MapleId);
                }
                    break;

                case QuestAction.ScriptStart:
                case QuestAction.ScriptEnd:
                {
                    npcId = iPacket.ReadInt();

                    var npc = Parent.Map.Npcs.Values.FirstOrDefault(loopNpc => loopNpc.MapleId == npcId);

                    if (npc == null)
                    {
                        return;
                    }

                    Parent.Converse(npc, quest);
                }
                    break;
            }
        }

        public void Start(QuestReference quest, int npcId)
        {
            Started.Add(quest.MapleId, new Dictionary<int, short>());

            foreach (var requiredKills in quest.PostRequiredKills)
            {
                Started[quest.MapleId].Add(requiredKills.Key, 0);
            }

            Parent.PrimaryStats.Experience += quest.ExperienceReward[0];
            Parent.PrimaryStats.Fame += (short)quest.FameReward[0];
            Parent.PrimaryStats.Meso += quest.MesoReward[0] * Parent.Client.Server.World.MesoRate;

            // TODO: Skill and pet rewards.

            foreach (var item in quest.PreItemRewards)
            {
                if (item.Value > 0)
                {
                    Parent.Items.Add(new Item(item.Key,
                        item.Value)); // TODO: Quest items rewards are displayed in chat.
                }
                else if (item.Value < 0)
                {
                    Parent.Items.Remove(item.Key, Math.Abs(item.Value));
                }
            }

            Update(quest.MapleId, QuestStatus.InProgress);

//            using (var pw = new PacketWriter(ServerOperationCode.QuestResult))
//            {
//                pw.WriteByte((byte)QuestResult.Complete);
//                pw.Writeint(quest.MapleId);
//                pw.WriteInt(npcId);
//                pw.WriteInt(0);
//
//                Parent.Send(pw);
//            }
        }

        public void Complete(QuestReference quest, int selection)
        {
            foreach (var item in quest.PostRequiredItems)
            {
                Parent.Items.Remove(item.Key, item.Value);
            }

            var mesoReward = quest.MesoReward[1] * Parent.Client.Server.World.MesoRate;


            Parent.PrimaryStats.Experience += quest.ExperienceReward[1];
            Parent.PrimaryStats.Fame += (short)quest.FameReward[1];
            Parent.PrimaryStats.Meso += mesoReward;

            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseExp, amount: quest.ExperienceReward[1],
                isWhite: true, inChat: true));
            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseFame, amount: quest.FameReward[1]));
            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, amount: mesoReward));

            foreach (var skill in quest.PostSkillRewards)
            {
                if (Parent.PrimaryStats.Job == skill.Value)
                {
                    Parent.Skills.Add(skill.Key);

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

                //    using (var pw = new PacketWriter(ServerOperationCode.Effect))
                //    {
                //        pw
                //            pw.WriteByte((byte)UserEffect.Quest)
                //            pw.WriteByte(1)
                //            pw.WriteInt(item.Key)
                //            pw.WriteInt(item.Value);

                //        this.Parent.Send(pw);
                //    }
                //}
                //else
                //{
                //    // TODO: Selective reward based on selection.
                //}
            }
            else
            {
                foreach (var item in quest.PostItemRewards)
                {
                    if (item.Value > 0)
                    {
                        Parent.Items.Add(new Item(item.Key, item.Value));
                    }
                    else if (item.Value < 0)
                    {
                        Parent.Items.Remove(item.Key, Math.Abs(item.Value));
                    }

                    using var pw = new PacketWriter(ServerOperationCode.Effect);
                    pw.WriteByte(UserEffect.Quest);
                    pw.WriteByte(1);
                    pw.WriteInt(item.Key);
                    pw.WriteInt(item.Value);

                    Parent.Send(pw);
                }
            }

            Update(quest.MapleId, QuestStatus.Complete);
            Delete(quest.MapleId);
            Completed.Add(quest.MapleId, DateTime.UtcNow);
            Parent.ShowLocalUserEffect(UserEffect.QuestComplete);
            Parent.ShowRemoteUserEffect(UserEffect.QuestComplete, true);
        }

        public void Forfeit(int questId)
        {
            Delete(questId);
            Update(questId, QuestStatus.NotStarted);
        }

        private void Update(int questId, QuestStatus status, string progress = "")
        {
            Parent.Send(GamePackets.ShowStatusInfo(MessageType.QuestRecord, mapleId: questId,
                questStatus: status, questString: progress));
        }

        public bool CanComplete(int questId, bool onlyOnFinalKill = false)
        {
            var quest = DataProvider.Quests.Data[questId];

            foreach (var requiredItem in quest.PostRequiredItems)
            {
                if (!Parent.Items.Contains(requiredItem.Key, requiredItem.Value))
                {
                    return false;
                }
            }

            foreach (var requiredQuest in quest.PostRequiredQuests)
            {
                if (!Completed.ContainsKey(requiredQuest))
                {
                    return false;
                }
            }

            foreach (var requiredKill in quest.PostRequiredKills)
            {
                if (onlyOnFinalKill)
                {
                    if (Started[questId][requiredKill.Key] != requiredKill.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Started[questId][requiredKill.Key] < requiredKill.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void NotifyComplete(int questId)
        {
//            using (var pw = new PacketWriter(ServerOperationCode.QuestClear))
//            {
//                pw.Writeint(questId);
//                Parent.Send(pw);
//            }
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteShort((short)Started.Count);

            foreach (var quest in Started)
            {
                pw.WriteInt(quest.Key);

                var kills = string.Empty;

                foreach (int kill in quest.Value.Values)
                {
                    kills += kill.ToString().PadLeft(3, '\u0030');
                }

                pw.WriteString(kills);
            }

            return pw.ToArray();
        }
    }
}
