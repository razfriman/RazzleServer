using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Login.Maple;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient>
    {
        public LoginCenterClient CenterConnection { get; set; }
        public Worlds Worlds { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public LoginServer()
        {
            for (byte i = 0; i < ServerConfig.Instance.Worlds.Count(); i++)
            {
                Worlds.Add(new World(ServerConfig.Instance.Worlds[i]));
            }

            StartCenterConnection(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
        }

        public override void ServerRegistered()
        {
            Log.LogInformation($"Registered Login Server.");
            Start(IPAddress.Loopback, Port);
        }

        public override void CenterServerConnected()
        {
            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Login);
            CenterConnection.Send(pw);
        }
    }
}