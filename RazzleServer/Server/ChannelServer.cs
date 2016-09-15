using System;
using System.Linq;
using System.Net;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Map;
using NLog;
using RazzleServer.Handlers;
using System.Collections.Generic;

namespace RazzleServer.Server
{
    public class ChannelServer : MapleServer
    {
        private Dictionary<int, MapleMap> _maps { get; set; } = new Dictionary<int, MapleMap>();
        public DateTime LastPing { get; set; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public ChannelServer(ushort port) {
            byte[] channelIp = new byte[] { 0, 0, 0, 0 };
            Start(new IPAddress(channelIp), port);
        }

        public void BroadCastPacket(PacketWriter pw)
        {
            foreach (MapleClient client in Clients.Values)
            {
                client.SendPacket(pw);
            }
        }

        public MapleMap GetMap(int mapID)
        {
            MapleMap ret;
            if (_maps.TryGetValue(mapID, out ret))
            {
                return ret;
            }

            // TODO - LOAD FROM WZ
            var map = new MapleMap(mapID, new Data.WZ.WzMap
            {
                Portals = new Dictionary<string, Data.WZ.WzMap.Portal>(),
                MobSpawnPoints = new List<Data.WZ.WzMap.MobSpawn>(),
                Npcs = new List<Data.WZ.WzMap.Npc>(),
                MapId = mapID,
                ReturnMap = 100000000,
            });
            _maps.Add(mapID, map);
            return map;
            // END TODO


            return null;
        }

        private void PingClients()
        {
            TimeSpan LastCheck = DateTime.UtcNow.Subtract(LastPing);
            foreach (MapleClient c in Clients.Values.Where(x => x.Account != null).ToList())
            {
                c.SendPacket(PongHandler.PingPacket());
                if (c.LastPong == DateTime.MinValue)
                {
                    c.LastPong = DateTime.UtcNow;
                }
                else
                {
                    TimeSpan timePassed = DateTime.UtcNow.Subtract(c.LastPong);
                    if (timePassed.TotalSeconds > ServerConfig.Instance.PingTimeout + LastCheck.TotalSeconds)
                        c.Disconnect("Ping timeout");
                }
            }
            LastPing = DateTime.UtcNow;
        }
    }
}