using System;
using System.Linq;
using System.Net;
using RazzleServer.Packet;
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
                MapleMap map = new MapleMap(kvp.Key, kvp.Value);
                _maps.Add(kvp.Key, map);
            }


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