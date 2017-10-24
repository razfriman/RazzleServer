using System.Collections.ObjectModel;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Util;

namespace RazzleServer.Login.Maple
{
    public sealed class World : KeyedCollection<byte, Channel>
    {
        public byte ID { get; private set; }
        public string Name { get; private set; }
        public ushort Port { get; private set; }
        public ushort ShopPort { get; private set; }
        public byte Channels { get; private set; }
        public WorldFlag Flag { get; private set; }
        public string EventMessage { get; private set; }
        public string TickerMessage { get; private set; }
        public bool AllowMultiLeveling { get; private set; }
        public int DefaultCreationSlots { get; private set; }
        public bool DisableCharacterCreation { get; private set; }
        public int ExperienceRate { get; private set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }

        public WorldStatus Status => WorldStatus.Normal;

        public int Population => this.Sum(x => x.Population);

        public Channel RandomChannel => this[Functions.Random(Count)];

        public World(byte id)
        {
            ID = id;

            var configSection = $"World{ID}";

            // TODO: Get the hardcoded values from config settings

            Name = Settings.GetString(configSection + "/Name");
            Port = (ushort)(8585 + 100 * ID);
            ShopPort = 9000;
            Channels = Settings.GetByte(configSection + "/Channels");
            Flag = Settings.GetEnum<WorldFlag>(configSection + "/Flag");
            EventMessage = Settings.GetString(configSection + "/EventMessage");
            TickerMessage = Settings.GetString(configSection + "/TickerMessage");
            AllowMultiLeveling = true;
            DefaultCreationSlots = 3;
            DisableCharacterCreation = false;
            ExperienceRate = Settings.GetInt(configSection + "/ExperienceRate");
            QuestExperienceRate = Settings.GetInt(configSection + "/QuestExperienceRate");
            PartyQuestExperienceRate = Settings.GetInt(configSection + "/PartyQuestExperienceRate");
            MesoRate = Settings.GetInt(configSection + "/MesoDropRate");
            DropRate = Settings.GetInt(configSection + "/ItemDropRate");
        }

        protected override void InsertItem(int index, Channel item)
        {
            base.InsertItem(index, item);

            Log.Success("Registered Channel {0}-{1}.", Name, item.ID);
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            base.RemoveItem(index);

            foreach (var loopChannel in this)
            {
                if (loopChannel.ID > index)
                {
                    loopChannel.ID--;
                }
            }

            Log.Warn("Unregistered Channel {0}-{1}.", Name, item.ID);
        }

        protected override byte GetKeyForItem(Channel item) => item.ID;
    }
}
