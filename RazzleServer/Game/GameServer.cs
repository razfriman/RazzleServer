using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using RazzleServer.Center;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Server;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient, GamePacketHandler>
    {
        public byte ChannelId { get; set; }
        public int Population { get; set; }
        public World World { get; set; }
        public Dictionary<int, Map> Maps { get; set; }
        public Map this[int id]
        {
            get
            {
                if (Maps.ContainsKey(id))
                {
                    return Maps[id];
                }

                // CREATE MAP AND SAVE IT TO DICT
                return null;
            }
        }

        public GameServer(ServerManager manager, World world, ushort port, byte channelId) : base(manager)
        {
            World = world;
            Port = port;
            ChannelId = channelId;
            Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);
        }

        public override void Dispose() => ShutDown();

        public Character GetCharacterById(int id) => Clients.Values.Select(x => x.Character).FirstOrDefault(x => x.Id == id);

        public Character GetCharacterByName(string name) => Clients.Values.Select(x => x.Character).FirstOrDefault(x => x.Name == name);

        public void Send(PacketWriter pw, GameClient except = null) => this.Clients.Values
        .Where(x => x.Key != except?.Key)
        .ToList()
        .ForEach(x => x.Send(pw));
    }
}
