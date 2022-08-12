using System;
using System.Linq;
using System.Text.Json.Serialization;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Server
{
    public sealed class World : MapleKeyedCollection<byte, IGameServer>
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public byte Channels { get; set; }
        public WorldStatusFlag Flag { get; set; }
        public string EventMessage { get; set; }
        public string TickerMessage { get; set; }
        public bool EnableCharacterCreation { get; set; }
        public int ExperienceRate { get; set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }
        public int EventDropRate { get; set; } = 100;
        public int EventExperienceRate { get; set; } = 100;
        public int MaxCharacterLimit { get; set; } = 2000;
        [JsonIgnore] public int Population => Values.Sum(x => x.Population);

        [JsonIgnore]
        public WorldStatus Status
        {
            get
            {
                var population = Population;
                var totalMax = MaxCharacterLimit * Channels;

                if (population >= totalMax)
                {
                    return WorldStatus.Full;
                }

                return population > totalMax / 2 ? WorldStatus.HighlyPopulated : WorldStatus.Normal;
            }
        }

        public World(WorldConfig config)
        {
            Id = config.Id;
            Name = config.Name;
            Channels = config.Channels;
            Flag = config.Flag;
            EventMessage = config.EventMessage;
            TickerMessage = config.TickerMessage;
            EnableCharacterCreation = config.EnableCharacterCreation;
            ExperienceRate = config.ExperienceRate;
            QuestExperienceRate = config.QuestExperienceRate;
            PartyQuestExperienceRate = config.PartyQuestExperienceRate;
            MesoRate = config.MesoRate;
            DropRate = config.DropRate;
        }

        public override byte GetKey(IGameServer item) => item.ChannelId;

        public void Send(PacketWriter pw, AClient except = null) => Values
            .SelectMany(x => x.Clients.Values)
            .Where(x => x.Key != except?.Key)
            .ToList()
            .ForEach(x => x.Send(pw));

        public Character GetCharacterById(int id) => Values
            .SelectMany(x => x.Clients.Values)
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Id == id);

        public Character GetCharacterByName(string name) => Values
            .SelectMany(x => x.Clients.Values)
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public SelectChannelResult CheckChannel(byte channel) =>
            Contains(channel) ? SelectChannelResult.Online : SelectChannelResult.Offline;

        public void UpdateTicker() => Send(GamePackets.Notify(TickerMessage, NoticeType.ScrollingText));
    }
}
