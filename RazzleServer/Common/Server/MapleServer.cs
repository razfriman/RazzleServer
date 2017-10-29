using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Util;
using MapleLib.PacketLib;

namespace RazzleServer.Server
{
    public abstract class MapleServer<T> where T : AClient, IDisposable
    {
        public Dictionary<string, T> Clients { get; set; } = new Dictionary<string, T>();
        public ushort Port;

        private TcpListener _listener;
        private const int BACKLOG_SIZE = 50;
        private bool _disposed;

        private static ILogger Log = LogManager.Log;

        public virtual void RemoveClient(T client)
        {
            if (Clients.ContainsKey(client.Key))
            {
                Clients.Remove(client.Key);
            }
        }

        public virtual void AddClient(T client)
        {
            if (!Clients.ContainsKey(client.Key))
            {
                Clients.Add(client.Key, client);
            }
            else
            {
                Log.LogError($"Client already exists with Key={client.Key}");
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

        public T GenerateClient(Socket socket)
        {
            string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            if (!AllowConnection(ip))
            {
                socket.Shutdown(SocketShutdown.Both);
                Log.LogWarning("Rejected Client");
                return null;
            }

            Log.LogInformation("Client Connected");

            var client = Activator.CreateInstance(typeof(T), new object[] { socket, this }) as T;

            try
            {
                client.SendHandshake();
                client.Key = ip + Functions.Random();
                AddClient(client);
                return client;
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error sending handshake. Disconnecting.");
                client.Terminate(e.ToString());
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
                    client.Terminate("Server is shutting down");
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

                GenerateClient(socket);
            }
        }
    }
}