using System.Linq;
using System.Net;
using RazzleServer.Login.Maple;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient>
    {
        public LoginCenterClient CenterConnection { get; set; }
        public Worlds Worlds { get; private set; }

        public LoginServer()
        {
            for (byte i = 0; i < ServerConfig.Instance.Worlds.Count(); i++)
            {
                Worlds.Add(new World(ServerConfig.Instance.Worlds[i]));
            }

            // start center server connection

            byte[] loginIp = { 0, 0, 0, 0 };
            Start(new IPAddress(loginIp), ServerConfig.Instance.LoginPort);
        }
    }
}