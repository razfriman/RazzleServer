using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using RazzleServer.Server;

namespace RazzleServer
{
    public sealed class World : AWorld
    {
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

        public override void Send(PacketWriter pw, AClient except = null) => Values
            .SelectMany(x => x.Clients.Values)
            .Where(x => x.Key != except?.Key)
            .ToList()
            .ForEach(x => x.Send(pw));

        public override Character GetCharacterById(int id) => Values
            .SelectMany(x => x.Clients.Values)
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Id == id);

        public override Character GetCharacterByName(string name) => Values
            .SelectMany(x => x.Clients.Values)
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public override SelectChannelResult CheckChannel(byte channel) =>
            Contains(channel) ? SelectChannelResult.Online : SelectChannelResult.Offline;

        public override void UpdateTicker() => Send(GamePackets.Notify(TickerMessage, NoticeType.ScrollingText));
    }
}
