using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Net;
using System.Net.Sockets;

namespace RazzleServer.Server
{
    public class LoginServer : MapleServer
    {
        public LoginServer()
        {
            var config = new ServerConfig();

            byte[] loginIp = new byte[] { 0, 0, 0, 0 };
            OnClientConnected += MapleClientConnect;
            Start(new IPAddress(loginIp), config.LoginPort);
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
    }
}