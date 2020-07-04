﻿using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Skills;
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
        }

        public void Save()
        {
        }

        public void Delete(int questId)
        {
            if (Started.ContainsKey(questId))
            {
                Started.Remove(questId);
            }
        }

        public void Delete()
        {
        }

        public void Start(QuestReference quest, int npcId, GameClient client)
        {
            Started.Add(quest.MapleId, new Dictionary<int, short>());

            foreach (var requiredKills in quest.PostRequiredKills)
            {
                Started[quest.MapleId].Add(requiredKills.Key, 0);
            }

            Parent.PrimaryStats.Experience += quest.ExperienceReward[0];
            Parent.PrimaryStats.Fame += (short)quest.FameReward[0];
            Parent.PrimaryStats.Meso += quest.MesoReward[0] * client.Server.World.MesoRate;

            // TODO: Skill and pet rewards.

            foreach (var (itemId, quantity) in quest.PreItemRewards)
            {
                if (quantity > 0)
                {
                    Parent.Items.Add(new Item(itemId, quantity)); // TODO: Quest items rewards are displayed in chat.
                }
                else if (quantity < 0)
                {
                    Parent.Items.Remove(itemId, Math.Abs(quantity));
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

        public void Complete(QuestReference quest, int selection, GameClient client)
        {
            foreach (var (itemId, quantity) in quest.PostRequiredItems)
            {
                Parent.Items.Remove(itemId, quantity);
            }

            var mesoReward = quest.MesoReward[1] * client.Server.World.MesoRate;


            Parent.PrimaryStats.Experience += quest.ExperienceReward[1];
            Parent.PrimaryStats.Fame += (short)quest.FameReward[1];
            Parent.PrimaryStats.Meso += mesoReward;

            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseExp, amount: quest.ExperienceReward[1],
                isWhite: true, inChat: true));
            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseFame, amount: quest.FameReward[1]));
            Parent.Send(GamePackets.ShowStatusInfo(MessageType.IncreaseMeso, amount: mesoReward));

            foreach (var (skillId, job) in quest.PostSkillRewards)
            {
                if (Parent.PrimaryStats.Job == job)
                {
                    if (!Parent.Skills.Contains(skillId))
                    {
                        Parent.Skills.Add(new Skill(skillId));
                    }
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
                foreach (var (itemId, quantity) in quest.PostItemRewards)
                {
                    if (quantity > 0)
                    {
                        Parent.Items.Add(new Item(itemId, quantity));
                    }
                    else if (quantity < 0)
                    {
                        Parent.Items.Remove(itemId, Math.Abs(quantity));
                    }

                    using var pw = new PacketWriter(ServerOperationCode.Effect);
                    pw.WriteByte(UserEffect.Quest);
                    pw.WriteByte(1);
                    pw.WriteInt(itemId);
                    pw.WriteInt(quantity);

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
            var quest = CachedData.Quests.Data[questId];

            foreach (var (slot, quantity) in quest.PostRequiredItems)
            {
                if (!Parent.Items.Contains(slot, quantity))
                {
                    return false;
                }
            }

            if (quest.PostRequiredQuests.Any(requiredQuest => !Completed.ContainsKey(requiredQuest)))
            {
                return false;
            }

            foreach (var (mapleId, value) in quest.PostRequiredKills)
            {
                if (onlyOnFinalKill)
                {
                    if (Started[questId][mapleId] != value)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Started[questId][mapleId] < value)
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

            foreach (var (questId, questData) in Started)
            {
                pw.WriteInt(questId);

                var kills = questData.Values.Cast<int>().Aggregate(string.Empty,
                    (current, kill) => current + kill.ToString().PadLeft(3, '\u0030'));

                pw.WriteString(kills);
            }

            return pw.ToArray();
        }
    }
}
