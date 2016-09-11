using System.Net;

namespace RazzleServer.Server
{
    public class LoginServer : MapleServer
    {
        public LoginServer()
        {
            byte[] loginIp = new byte[] { 0, 0, 0, 0 };
            Start(new IPAddress(loginIp), ServerConfig.Instance.LoginPort);
        }
    }
}