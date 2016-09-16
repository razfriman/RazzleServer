using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Packet;
using RazzleServer.Server;
using RazzleServer.Net;
using RazzleServer.Util;
using NLog;
using RazzleServer.Scripts;

namespace RazzleServer.Player
{
    public class MapleClient
    {
        public static Dictionary<CMSGHeader,List<APacketHandler>> PacketHandlers = new Dictionary<CMSGHeader,List<APacketHandler>>();
        
        public string Host { get; set; }
        public int Port { get; set; }
        public ClientSocket Socket { get; set; }
        public MapleAccount Account { get; set; }
        public byte Channel { get; set; }
        public bool Connected { get; set; }
        public DateTime LastPong { get; set; }
        public MapleServer Server{get;set;}
        public string Key {get;set;}
        public NpcEngine NpcEngine { get; set; }

        private static Logger Log = LogManager.GetCurrentClassLogger();


        public MapleClient(Socket session, MapleServer server)
        {
            Socket = new ClientSocket(this, session);
            Server = server;
            Host = Socket.Host;
            Port = Socket.Port;
            Channel = 0;
            Connected = true;
        }

        public static void RegisterPacketHandlers() {
            
            var types = Assembly.GetEntryAssembly().GetTypes();

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attribute = type.GetTypeInfo().GetCustomAttribute<PacketHandlerAttribute>();

                if(attribute != null) {
                    var header = attribute.Header;

                    if(!PacketHandlers.ContainsKey(header)) {
                        PacketHandlers[header] = new List<APacketHandler>();
                    }

                    handlerCount++;
                    var handler = (APacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.Info($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.Info($"Registered {handlerCount} packet handlers");
        }

        internal void RecvPacket(PacketReader packet)
        {
            CMSGHeader header = CMSGHeader.UNKNOWN;
            try
            {
                if(packet.Available >= 2) {
                    header = (CMSGHeader) packet.ReadHeader();

                    if (PacketHandlers.ContainsKey(header)) {
                        Log.Debug($"Recevied [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in PacketHandlers[header]) {
                            handler.HandlePacket(packet, this);
                        }
                    } else {
                        Log.Warn($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        public void Disconnect(string reason, params object[] values)
        {
            Log.Info($"Disconnected client with reason: {string.Format(reason, values)}");

            if (Socket != null) {
                Socket.Disconnect();
            }
        }

        internal void Disconnected()
        {
            var save = Account?.Character; ;
            try
            {
                Account?.Release();
                Connected = false;
                Server.RemoveClient(this);
                save?.LoggedOut();
                NpcEngine?.Dispose();
                Socket?.Dispose();
            } catch (Exception e) {
                Log.Error(e, $"Error while disconnecting. Account [{Account?.Name}] Character [{save?.Name}]");
            }
        }

        public void SendPacket(PacketWriter packet)
        {
            if (ServerConfig.Instance.PrintPackets)
            {
                Log.Debug($"Sending: {Functions.ByteArrayToStr(packet.ToArray())}");
            }

            if (Socket == null) return;

            Socket.SendPacket(packet);
        }
        
        public void SendHandshake()
        {
            if (Socket == null) return;

            uint sIV = Functions.RandomUInt();
            uint rIV = Functions.RandomUInt();

            Socket.Crypto.SetVectors(sIV, rIV);

            PacketWriter writer = new PacketWriter(0x0E);
            writer.WriteUShort(ServerConfig.Instance.Version);
            writer.WriteMapleString(ServerConfig.Instance.SubVersion.ToString());
            writer.WriteUInt(rIV);
            writer.WriteUInt(sIV);
            writer.WriteByte(ServerConfig.Instance.ServerType);
            Socket.SendRawPacket(writer.ToArray());
        }
    }
}