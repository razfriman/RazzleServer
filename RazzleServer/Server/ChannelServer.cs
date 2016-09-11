using System;
using System.Net;
using System.Net.Sockets;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using RazzleServer.Map;
using NLog;

namespace RazzleServer.Server
{
    public class ChannelServer : MapleServer
    {
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
            return new MapleMap();
        }
    }
}