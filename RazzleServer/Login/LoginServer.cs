using System.Linq;
using System.Net;
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
            Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);
        }

        public override void CenterServerConnected()
        {
            CenterConnection = new LoginCenterClient(this, _centerSocket);

            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Login);
            CenterConnection.Send(pw);
        }
    }
}