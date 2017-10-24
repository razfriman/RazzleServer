using System.Net;
using Destiny.Maple;
using RazzleServer.Login;

namespace RazzleServer.Server
{
    public class LoginServer : MapleServer
    {
        public static CenterServer CenterConnection { get; set; }
        public static Worlds Worlds { get; private set; }

        public static bool AutoRegister { get; private set; }
        public static bool RequestPin { get; private set; }
        public static bool RequestPic { get; private set; }
        public static bool RequireStaffIP { get; private set; }
        public static int MaxCharacters { get; private set; }

        public LoginServer()
        {

            for (byte i = 0; i < ServerConfig.Instance.Worlds; i++)
            {
                Worlds.Add(new World(i));
            }

            byte[] loginIp = { 0, 0, 0, 0 };
            Start(new IPAddress(loginIp), ServerConfig.Instance.LoginPort);



        }
    }
}