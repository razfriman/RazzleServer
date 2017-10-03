using System.Net;

namespace RazzleServer.Server
{
    public class LoginServer : MapleServer
    {
        public LoginServer()
        {
            byte[] loginIp = { 0, 0, 0, 0 };
            Start(new IPAddress(loginIp), ServerConfig.Instance.LoginPort);
        }
    }
}