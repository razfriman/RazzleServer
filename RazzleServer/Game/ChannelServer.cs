using System;
using System.Linq;
using System.Net;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Map;
using Microsoft.Extensions.Logging;
using RazzleServer.Handlers;
using System.Collections.Generic;
using RazzleServer.Data.WZ;
using RazzleServer.Data;
using RazzleServer.Util;

namespace RazzleServer.Server
{
    public class ChannelServer : MapleServer
    {
        private Dictionary<int, MapleMap> _maps { get; set; } = new Dictionary<int, MapleMap>();
        public DateTime LastPing { get; set; }

        private static ILogger Log = LogManager.Log;

        public ChannelServer(ushort port) {

            _maps = new Dictionary<int, MapleMap>();
            foreach (KeyValuePair<int, WzMap> kvp in DataBuffer.MapBuffer)
            {
                var map = new MapleMap(kvp.Key, kvp.Value);
                _maps.Add(kvp.Key, map);
            }


            byte[] channelIp = { 0, 0, 0, 0 };
            Start(new IPAddress(channelIp), port);
        }

        public void BroadCastPacket(PacketWriter pw)
        {
            foreach (var client in Clients.Values)
            {
                client.Send(pw);
            }
        }

        public MapleMap GetMap(int mapID) => _maps.TryGetValue(mapID, out var ret) ? ret : null;

        private void PingClients()
        {
            TimeSpan LastCheck = DateTime.UtcNow.Subtract(LastPing);
            foreach (var c in Clients.Values.Where(x => x.Account != null).ToList())
            {
                c.Send(PongHandler.PingPacket());
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