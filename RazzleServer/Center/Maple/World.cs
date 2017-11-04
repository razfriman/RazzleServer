using System.Collections.ObjectModel;
using System.Linq;
using RazzleServer.Game;
using RazzleServer.Common.Constants;
using Newtonsoft.Json;

namespace RazzleServer.Center.Maple
{
    public sealed class World : KeyedCollection<byte, GameServer>
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public byte Channels { get; set; }
        public WorldStatusFlag Flag { get; set; }
        public string EventMessage { get; set; }
        public string TickerMessage { get; set; }
        public bool EnableCharacterCreation { get; set; }
        public bool EnableMultiLeveling { get; set; }
        public int ExperienceRate { get; set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }

        public World(WorldConfig config)
        {
            ID = config.ID;
            Name = config.Name;
            Channels = config.Channels;
            Flag = config.Flag;
            EventMessage = config.EventMessage;
            TickerMessage = config.TickerMessage;
            EnableCharacterCreation = config.EnableCharacterCreation;
            EnableMultiLeveling = config.EnableMultiLeveling;
            ExperienceRate = config.ExperienceRate;
            QuestExperienceRate = config.QuestExperienceRate;
            PartyQuestExperienceRate = config.PartyQuestExperienceRate;
            MesoRate = config.MesoRate;
            DropRate = config.DropRate;
        }

        [JsonIgnore]
        public bool IsFull => Count == Channels;

        [JsonIgnore]
        public int Population => this.Sum(x => x.Population);

        protected override byte GetKeyForItem(GameServer item) => item.ChannelID;
    }
}
