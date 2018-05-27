using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
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
            Parent = parent;

            Started = new Dictionary<ushort, Dictionary<int, short>>();
            Completed = new Dictionary<ushort, DateTime>();
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("quests_started").Populate("CharacterId = {0}", this.Parent.Id))
            //{
            //    if (!this.Started.ContainsKey((ushort)datum["QuestId"]))
            //    {
            //        this.Started.Add((ushort)datum["QuestId"], new Dictionary<int, short>());
            //    }

            //    if (datum["MobId"] != null && datum["Killed"] != null)
            //    {
            //        this.Started[(ushort)datum["QuestId"]].Add((int)datum["MobId"], ((short)datum["Killed"]));
            //    }
            //}
        }

        public void Save()
        {
            //foreach (KeyValuePair<ushort, Dictionary<int, short>> loopStarted in this.Started)
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

            //foreach (KeyValuePair<ushort, DateTime> loopCompleted in this.Completed)
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

        public void Delete(ushort questId)
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
            var questId = iPacket.ReadUShort();

            if (!DataProvider.Quests.Data.ContainsKey(questId))
            {
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

            Parent.Experience += quest.ExperienceReward[0];
            Parent.Fame += (short)quest.FameReward[0];
            Parent.Meso += quest.MesoReward[0] * Parent.Client.Server.World.MesoRate;

            // TODO: Skill and pet rewards.

            foreach (var item in quest.PreItemRewards)
            {
                if (item.Value > 0)
                {
                    Parent.Items.Add(new Item(item.Key, item.Value)); // TODO: Quest items rewards are displayed in chat.
                }
                else if (item.Value < 0)
                {
                    Parent.Items.Remove(item.Key, Math.Abs(item.Value));
                }
            }

            Update(quest.MapleId, QuestStatus.InProgress);

            using (var oPacket = new PacketWriter(ServerOperationCode.QuestResult))
            {
                oPacket.WriteByte((byte)QuestResult.Complete);
                oPacket.WriteUShort(quest.MapleId);
                oPacket.WriteInt(npcId);
                oPacket.WriteInt(0);

                Parent.Client.Send(oPacket);
            }
        }

        public void Complete(QuestReference quest, int selection)
        {
            foreach (var item in quest.PostRequiredItems)
            {
                Parent.Items.Remove(item.Key, item.Value);
            }

            Parent.Experience += quest.ExperienceReward[1];

            using (var oPacket = new PacketWriter(ServerOperationCode.Message))
            {
                oPacket.WriteByte((byte)MessageType.IncreaseExp);
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

                Parent.Client.Send(oPacket);
            }

            Parent.Fame += (short)quest.FameReward[1];

            // TODO: Fame gain packet in chat.

            Parent.Meso += quest.MesoReward[1] * Parent.Client.Server.World.MesoRate;

            // TODO: Meso gain packet in chat.

            foreach (var skill in quest.PostSkillRewards)
            {
                if (Parent.Job == skill.Value)
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

                    using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
                    {
                        oPacket.WriteByte((byte)UserEffect.Quest);
                        oPacket.WriteByte(1);
                        oPacket.WriteInt(item.Key);
                        oPacket.WriteInt(item.Value);

                        Parent.Client.Send(oPacket);
                    }
                }
            }

            Update(quest.MapleId, QuestStatus.Complete);

            Delete(quest.MapleId);

            Completed.Add(quest.MapleId, DateTime.UtcNow);

            Parent.ShowLocalUserEffect(UserEffect.QuestComplete);
            Parent.ShowRemoteUserEffect(UserEffect.QuestComplete, true);
        }

        public void Forfeit(ushort questId)
        {
            Delete(questId);

            Update(questId, QuestStatus.NotStarted);
        }

        private void Update(ushort questId, QuestStatus status, string progress = "")
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Message))
            {
                oPacket.WriteByte((byte)MessageType.QuestRecord);
                oPacket.WriteUShort(questId);
                oPacket.WriteByte((byte)status);

                if (status == QuestStatus.InProgress)
                {
                    oPacket.WriteString(progress);
                }
                else if (status == QuestStatus.Complete)
                {
                    oPacket.WriteDateTime(DateTime.Now);
                }

                Parent.Client.Send(oPacket);
            }
        }

        public bool CanComplete(ushort questId, bool onlyOnFinalKill = false)
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

        public void NotifyComplete(ushort questId)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.QuestClear))
            {
                oPacket.WriteUShort(questId);

                Parent.Client.Send(oPacket);
            }
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteShort((short)Started.Count);

                foreach (var quest in Started)
                {
                    oPacket.WriteUShort(quest.Key);

                    var kills = string.Empty;

                    foreach (int kill in quest.Value.Values)
                    {
                        kills += kill.ToString().PadLeft(3, '\u0030');
                    }

                    oPacket.WriteString(kills);
                }

                oPacket.WriteShort((short)Completed.Count);

                foreach (var quest in Completed)
                {
                    oPacket.WriteUShort(quest.Key);
                    oPacket.WriteDateTime(quest.Value);
                }

                return oPacket.ToArray();
            }
        }
    }
}

