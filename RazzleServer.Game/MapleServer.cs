using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using RazzleServer.Common.Util;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game
{
    public abstract class MapleServer<TClient, TPacketHandler> : IMapleServer
        where TClient : AClient
        where TPacketHandler : APacketHandler<TClient>
    {
        public Dictionary<string, AClient> Clients { get; set; } = new Dictionary<string, AClient>();

        public Dictionary<ClientOperationCode, List<TPacketHandler>> PacketHandlers { get; } =
            new Dictionary<ClientOperationCode, List<TPacketHandler>>();

        public HashSet<ClientOperationCode> IgnorePacketPrintSet { get; } = new HashSet<ClientOperationCode>();

        public ushort Port { get; set; }
        public IServerManager Manager { get; set; }
        private TcpListener _listener;
        private bool _disposed;
        private const int BacklogSize = 50;
        public abstract ILogger Logger { get; }

        protected MapleServer(IServerManager manager)
        {
            Manager = manager;
            RegisterPacketHandlers();
            RegisterIgnorePacketPrints();
        }

        public void RemoveClient(TClient client)
        {
            if (Clients.ContainsKey(client.Key))
            {
                Clients.Remove(client.Key);
            }
        }

        public void AddClient(TClient client)
        {
            if (!Clients.ContainsKey(client.Key))
            {
                Clients.Add(client.Key, client);
            }
            else
            {
                Logger.Error($"Client already exists with Key={client.Key}");
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown();
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool AllowConnection(string address) => true;

        public async Task<TClient> GenerateClient(Socket socket)
        {
            var ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            if (!AllowConnection(ip))
            {
                socket.Shutdown(SocketShutdown.Both);
                Logger.Warning("Rejected Client");
                return null;
            }

            Logger.Information("Client Connected");

            var client = Activator.CreateInstance(typeof(TClient), socket, this) as TClient;

            try
            {
                if (client == null)
                {
                    return null;
                }

                await client.SendHandshake();
                client.Key = $"{ip}-{Functions.Random()}";
                AddClient(client);
                return client;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error sending handshake. Disconnecting.");
                client?.Terminate(e.ToString());
                RemoveClient(client);
                return null;
            }
        }

        public virtual void Shutdown()
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
                Logger.Error("Error during server shutdown", e);
            }
        }

        public void Start(IPAddress ip, ushort port)
        {
            try
            {
                Logger.Information($"Starting {GetType().Name.PadRight(11)} on port [{port}]");
                Port = port;
                _listener = new TcpListener(ip, port);
                _listener.Start(BacklogSize);
            }
            catch (Exception e)
            {
                Logger.Fatal(e, $"Error starting server on port [{port}]");
                Shutdown();
            }

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

                await GenerateClient(socket);
            }
        }

        public void RegisterIgnorePacketPrints()
        {
            var type = typeof(ClientOperationCode);
            var members = type.GetMembers();

            foreach (var member in members)
            {
                var isIgnored = member.GetCustomAttributes(typeof(IgnorePacketPrintAttribute), false).Any();

                if (isIgnored && Enum.TryParse(type, member.Name, out var result))
                {
                    IgnorePacketPrintSet.Add((ClientOperationCode)result);
                }
            }
        }

        public void RegisterPacketHandlers()
        {
            var types = Assembly.GetAssembly(typeof(TPacketHandler))
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(TPacketHandler)));

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo()
                    .GetCustomAttributes()
                    .OfType<PacketHandlerAttribute>()
                    .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<TPacketHandler>();
                    }

                    var handler = (TPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                }
            }
        }
    }
}
