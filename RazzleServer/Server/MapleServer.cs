using RazzleServer.Player;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RazzleServer.Server
{
    public abstract class MapleServer : IDisposable
    {
        public Dictionary<string, MapleClient> Clients { get; set; } = new Dictionary<string, MapleClient>();

        public delegate void ClientConnectedHandler(Socket client);
        public event ClientConnectedHandler OnClientConnected;

        public ushort Port = 0;

        private TcpListener Listener;
        private const int BACKLOG_SIZE = 50;
        private bool Disposed = false;

        public MapleServer()
        {
        }

        public virtual void RemoveClient(MapleClient client)
        {
            if(Clients.ContainsKey(client.Key)) {
                Clients.Remove(client.Key);
            }
        }

        ~MapleServer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ShutDown();
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void ShutDown()
        {
            try
            {
                Disposed = true;
                Listener.Stop();
                Listener.Server.Shutdown(SocketShutdown.Both);
            }
            catch { }
        }

        public void Start(IPAddress ip, ushort port)
        {
            System.Console.WriteLine($"Starting Server on port {port}");
            Port = port;
            Listener = new TcpListener(ip, port);
            Listener.Start(BACKLOG_SIZE);

            Task.Factory.StartNew(ListenLoop);
        }

        private async void ListenLoop()
        {
            while (true)
            {
                var socket = await Listener.AcceptSocketAsync();

                if (socket == null || Disposed)
                {
                    break;
                }

                if (OnClientConnected != null)
                {
                    OnClientConnected(socket);
                }
            }
        }
    }
}