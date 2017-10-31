using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Network;
using RazzleServer.Common.Util;

namespace RazzleServer.Server
{
    public abstract class MapleServer<T> where T : AClient, IDisposable
    {
        public Dictionary<string, T> Clients { get; set; } = new Dictionary<string, T>();
        public ushort Port;

        private TcpListener _listener;
        protected Socket _centerSocket;
        private bool _disposed;
        private const int BACKLOG_SIZE = 50;

        protected static readonly ILogger Log = LogManager.Log;

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

        public virtual void CenterServerConnected()
        {

        }

        public virtual void ServerRegistered()
        {

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

        public void StartCenterConnection(IPAddress ip, ushort port)
        {
            bool connected = false;
            int tries = 0;

            while (!connected && tries < 5)
            {
                try
                {
                    _centerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _centerSocket.Connect(ip, port);
                    connected = true;

                    Log.LogInformation("Connected");
                }
                catch (Exception e)
                {
                    Log.LogWarning(e, $"Could not connect to Center Server at {ip}:{port}");
                    tries++;
                    Thread.Sleep(5000);
                }
            }

            if (connected)
            {
                CenterServerConnected();

            }
            else
            {
                Log.LogCritical($"Connection to Center Server failed at {ip}:{port}");

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