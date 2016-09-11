using System;
using System.Net;
using System.Net.Sockets;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using RazzleServer.Map;

namespace RazzleServer.Server
{
    public class ChannelServer : MapleServer
    {
        public ChannelServer(ushort port) {
            byte[] loginIp = new byte[] { 0, 0, 0, 0 };
            OnClientConnected += MapleClientConnect;
            Start(new IPAddress(loginIp), port);
        }

        private bool AllowConnection(string address)
        {
            return true;
        }

        private void MapleClientConnect(Socket socket)
        {            
            string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            if (!AllowConnection(ip))
            {
                socket.Shutdown(SocketShutdown.Both);
                System.Console.WriteLine("Rejected Client");
                return;
            }

            System.Console.WriteLine("Client Connected");

            MapleClient Client = new MapleClient(socket, this);
            try
            {
                Client.SendHandshake();
                Clients.Add(ip + Functions.Random(), Client);
                System.Console.WriteLine("Client: " + Clients.Count);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Disconnect " + e.Message);
                Client.Disconnect(e.ToString());
            }
        }

        public override void ShutDown() {
            foreach(var client in Clients.Values) {
                client.Disconnect("Channel Server is shutting down");
            }
            base.ShutDown();
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