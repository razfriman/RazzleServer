using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;

namespace RazzleServer.Login.Maple
{
    public sealed class World : KeyedCollection<byte, Channel>
    {
        public byte ID { get; private set; }
        public string Name { get; private set; }
        public ushort Port { get; private set; }
        public ushort ShopPort { get; private set; }
        public byte ChannelCount { get; private set; }
        public WorldStatusFlag Flag { get; private set; }
        public string EventMessage { get; private set; }
        public string TickerMessage { get; private set; }
        public bool AllowMultiLeveling { get; private set; }
        public int DefaultCreationSlots { get; private set; }
        public bool EnableCharacterCreation { get; private set; }
        public bool EnableMultiLeveling { get; private set; }
        public int ExperienceRate { get; private set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }

        [JsonIgnore]
        public Dictionary<byte, Channel> Channels { get; set; }

        [JsonIgnore]
        public WorldStatus Status => WorldStatus.Normal;

        [JsonIgnore]
        public int Population => this.Sum(x => x.Population);

        [JsonIgnore]
        public Channel RandomChannel => this[Functions.Random(Count)];

        protected override byte GetKeyForItem(Channel item) => item.ID;
    }
}
