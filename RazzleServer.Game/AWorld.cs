using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game
{
    public abstract class AWorld : MapleKeyedCollection<byte, IGameServer>
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


        public abstract void Send(PacketWriter pw, AClient except = null);

        public abstract Character GetCharacterById(int id);

        public abstract Character GetCharacterByName(string name);

        public abstract SelectChannelResult CheckChannel(byte channel);

        public abstract void UpdateTicker();
    }
}
