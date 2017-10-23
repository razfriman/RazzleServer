using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Packet;
using RazzleServer.Server;
using RazzleServer.Util;
using Microsoft.Extensions.Logging;
using RazzleServer.Scripts;
using MapleLib.PacketLib;

namespace RazzleServer.Player
{
    public class MapleClient : IClient
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

        private static ILogger Log = LogManager.Log;


        public MapleClient(Socket session, MapleServer server)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AESKey);
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
                    Log.LogInformation($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }

        public void RecvPacket(PacketReader packet)
        {
            CMSGHeader header = CMSGHeader.UNKNOWN;
            try
            {
                if(packet.Available >= 2) {
                    header = (CMSGHeader) packet.ReadUShort();

                    if (PacketHandlers.ContainsKey(header)) {
                        Log.LogDebug($"Recevied [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in PacketHandlers[header]) {
                            handler.HandlePacket(packet, this);
                        }
                    } else {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        public void Disconnect(string reason, params object[] values)
        {
            Log.LogInformation($"Disconnected client with reason: {string.Format(reason, values)}");

            if (Socket != null) {
                Socket.Disconnect();
       }
        }

        public void Disconnected()
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
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Name}] Character [{save?.Name}]");
            }
        }

        public void SendPacket(PacketWriter packet)
        {
            if (ServerConfig.Instance.PrintPackets)
            {
                Log.LogDebug($"Sending: {Functions.ByteArrayToStr(packet.ToArray())}");
            }

            if (Socket == null) return;

            Socket.SendPacket(packet);
        }
        
        public void SendHandshake()
        {
            if (Socket == null) return;

            var sIV = Functions.RandomUInt();
            var rIV = Functions.RandomUInt();

            Socket.Crypto.SetVectors(sIV, rIV);

            var writer = new PacketWriter();
            writer.WriteUShort(0x0E);
            writer.WriteUShort(ServerConfig.Instance.Version);
            writer.WriteMapleString(ServerConfig.Instance.SubVersion.ToString());
            writer.WriteUInt(rIV);
            writer.WriteUInt(sIV);
            writer.WriteByte(ServerConfig.Instance.ServerType);
            Socket.SendRawPacket(writer.ToArray());
        }
    }
}