using RazzleServer.Player;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Util;

namespace RazzleServer.Server
{
    public abstract class MapleServer : IDisposable
    {
        public Dictionary<string, MapleClient> Clients { get; set; } = new Dictionary<string, MapleClient>();
        public ushort Port;

        private TcpListener _listener;
        private const int BACKLOG_SIZE = 50;
        private bool _disposed;

        private static ILogger Log = LogManager.Log;

        public virtual void RemoveClient(MapleClient client)
        {
            if (Clients.ContainsKey(client.Key))
            {
                Clients.Remove(client.Key);
            }
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


        public virtual bool AllowConnection(string address) => true;

        public virtual MapleClient CreateMapleClient(Socket socket)
        {
            string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            if (!AllowConnection(ip))
            {
                socket.Shutdown(SocketShutdown.Both);
                Log.LogWarning("Rejected Client");
                return null;
            }

            Log.LogInformation("Client Connected");

            MapleClient client = new MapleClient(socket, this);
            try
            {
                client.SendHandshake();
                client.Key = ip + Functions.Random();
                Clients.Add(client.Key, client);
                return client;
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error sending handshake. Disconnecting.");
                client.Disconnect(e.ToString());
                RemoveClient(client);
                return null;
            }
        }

        public virtual void ShutDown()
        {
            try
            {
                foreach (var client in Clients.Values)
                {
                    client.Disconnect("Server is shutting down");
                }

                _disposed = true;
                _listener.Stop();
                _listener.Server.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Log.LogError("Error during server shutdown", e);
            }
        }

        public void Start(IPAddress ip, ushort port)
        {
            Log.LogInformation($"Starting server on port [{port}]");
            Port = port;
            _listener = new TcpListener(ip, port);
            _listener.Start(BACKLOG_SIZE);

            Task.Factory.StartNew(ListenLoop);
        }

        private async Task ListenLoop()
        {
            while (true)
            {
                var socket = await _listener.AcceptSocketAsync();

                if (socket == null || _disposed)
                {
                    break;
                }

                CreateMapleClient(socket);
            }
        }
    }
}