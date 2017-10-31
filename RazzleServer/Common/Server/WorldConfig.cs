using System;
using RazzleServer.Common.Constants;

namespace RazzleServer.Common.Server
{
    public class WorldConfig
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public ushort Port { get; set; }
        public ushort ShopPort { get; set; }
        public byte Channels { get; set; }
        public WorldStatusFlag Flag { get; set; }
        public string EventMessage { get; set; }
        public string TickerMessage { get; set; }
        public bool AllowMultiLeveling { get; set; }
        public int DefaultCreationSlots { get; set; }
        public bool EnableCharacterCreation { get; set; }
        public bool EnableMultiLeveling { get; set; }
        public int ExperienceRate { get; set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }
    }
}
