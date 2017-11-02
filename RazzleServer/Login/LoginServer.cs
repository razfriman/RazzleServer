using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient>
    {
        public LoginCenterClient CenterConnection { get; set; }
        public Worlds Worlds { get; private set; } = new Worlds();
        public Dictionary<ClientOperationCode, List<LoginPacketHandler>> PacketHandlers { get; private set; } = new Dictionary<ClientOperationCode, List<LoginPacketHandler>>();

        public LoginServer()
        {
            for (byte i = 0; i < ServerConfig.Instance.Worlds.Count(); i++)
            {
                Worlds.Add(new World(ServerConfig.Instance.Worlds[i]));
            }
            Port = ServerConfig.Instance.LoginPort;
            StartCenterConnection(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
        }

        public override void ServerRegistered() => Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);

        public override void CenterServerConnected()
        {
            CenterConnection = new LoginCenterClient(this, _centerSocket);
            CenterConnection.Socket.Crypto.HandshakeFinished += (SIV, RIV) => SendRegistrationRequest();
        }

        private void SendRegistrationRequest()
        {
            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Login);
            pw.WriteByte((byte)Worlds.Count);

            foreach (var loopWorld in Worlds)
            {
                pw.WriteByte(loopWorld.ID);
                pw.WriteString(loopWorld.Name);
                pw.WriteUShort(loopWorld.Port);
                pw.WriteUShort(loopWorld.ShopPort);
                pw.WriteByte(loopWorld.Channels);
                pw.WriteString(loopWorld.TickerMessage);
                pw.WriteBool(loopWorld.EnableMultiLeveling);
                pw.WriteInt(loopWorld.ExperienceRate);
                pw.WriteInt(loopWorld.QuestExperienceRate);
                pw.WriteInt(loopWorld.PartyQuestExperienceRate);
                pw.WriteInt(loopWorld.MesoRate);
                pw.WriteInt(loopWorld.DropRate);
            }

            CenterConnection?.Send(pw);
        }

        public override void RegisterPacketHandlers()
        {

            var types = Assembly.GetEntryAssembly()
                                .GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(LoginPacketHandler)));

            var handlerCount = 0;

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
                        PacketHandlers[header] = new List<LoginPacketHandler>();
                    }

                    handlerCount++;
                    var handler = (LoginPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.LogDebug($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }
    }
}