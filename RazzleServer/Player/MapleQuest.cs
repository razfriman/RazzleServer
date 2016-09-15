using RazzleServer.Data.WZ;
using RazzleServer.Packet;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Player
{
    public class MapleQuest
    {
        public MapleQuestStatus State { get; set; }
        public WzQuest QuestInfo { get; private set; }
        public string Data { get; set; }
        public Dictionary<int, int> MonsterKills { get; set; }

        public MapleQuest(WzQuest info, MapleQuestStatus state = MapleQuestStatus.InProgress, string data = "", Dictionary<int, int> monsterData = null)
        {
            QuestInfo = info;
            State = state;
            Data = data;
            if (monsterData != null)
                MonsterKills = monsterData;
            else
                MonsterKills = new Dictionary<int, int>();
            if (info != null)
            {
                foreach (var fr in info.FinishRequirements) //pre-load mob kill requirements
                {
                    if (fr.Type == QuestRequirementType.mob)
                    {
                        WzQuestIntegerPairRequirement mobReq = (WzQuestIntegerPairRequirement)fr;
                        foreach (var pair in mobReq.Data)
                        {
                            int mobId = pair.Key;
                            if (!MonsterKills.ContainsKey(mobId))
                                MonsterKills.Add(mobId, 0);
                        }
                    }
                }
            }
        }

        #region packets
        public PacketWriter Update()
        {
            var pw = new PacketWriter(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteByte(1);
            pw.WriteUShort(25672);
            pw.WriteByte((byte)State);
            switch (State)
            {
                case MapleQuestStatus.NotStarted:
                    pw.WriteByte(0);
                    break;
                case MapleQuestStatus.InProgress:
                    pw.WriteMapleString(Data);
                    break;
                case MapleQuestStatus.Completed:
                    pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(DateTime.UtcNow));
                    break;
            }
            return pw;
        }

        public PacketWriter UpdateFinish(int questId, int npcId, int nextQuest = 0)
        {
            
            var pw = new PacketWriter(SMSGHeader.UPDATE_QUEST_INFO);

            pw.WriteByte(0xA);
            pw.WriteUShort((ushort)questId);
            pw.WriteInt(npcId);
            pw.WriteInt(nextQuest);

            return pw;
        }

        public PacketWriter UpdateMobKillProgress()
        {
            
            var pw = new PacketWriter(SMSGHeader.SHOW_STATUS_INFO);

            pw.WriteByte(1);
            pw.WriteUShort(QuestInfo.Id);
            pw.WriteByte(1);
            pw.WriteMapleString(GetMobKillsInfoString());
            return pw;
        }

        public static PacketWriter ShowQuestCompleteNotice(ushort questId)
        {
            
            var pw = new PacketWriter(SMSGHeader.SHOW_QUEST_COMPLETION);
            pw.WriteUShort(questId);
            return pw;
        }
        #endregion

        #region Functions
        private string GetMobKillsInfoString()
        {
            string ret = "";
            foreach (var pair in MonsterKills)
            {
                string strValue = pair.Value.ToString();
                if (strValue.Length < 3)
                {
                    int fillAmount = 3 - strValue.Length;
                    for (int i = 0; i < fillAmount; i++)
                        ret += '0';
                }
                ret += strValue;
            }
            return ret;
        }

        public void KilledMob(MapleClient c, int mobId)
        {
            int currentKills;
            if (MonsterKills.TryGetValue(mobId, out currentKills))
            {
                int requiredKills = GetRequiredMobKillAmount(mobId);
                if (currentKills < requiredKills)
                {
                    currentKills++;
                    MonsterKills[mobId] = currentKills;
                    c.SendPacket(UpdateMobKillProgress());
                    if (currentKills == requiredKills)
                    {
                        c.SendPacket(ShowQuestCompleteNotice(QuestInfo.Id));
                    }
                }
            }
        }

        private int GetRequiredMobKillAmount(int mobId)
        {
            foreach (WzQuestRequirement req in QuestInfo.FinishRequirements)
            {
                if (req.Type == QuestRequirementType.mob)
                {
                    return ((WzQuestIntegerPairRequirement)req).Data.FirstOrDefault(p => p.Key == mobId).Value;
                }
            }
            return 0;
        }

        public void Forfeit()
        {
            Data = "";
            foreach (var kvp in this.MonsterKills)
            {
                MonsterKills[kvp.Key] = 0;
            }
            State = MapleQuestStatus.NotStarted;
        }

        public bool HasMonsterKillObjectives { get { return this.MonsterKills.Any(); } }
        #endregion
    }
}