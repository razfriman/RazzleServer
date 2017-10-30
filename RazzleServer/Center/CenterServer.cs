﻿using System.Net;
using RazzleServer.Center.Maple;
using RazzleServer.Server;

namespace RazzleServer.Center
{
    public class CenterServer : MapleServer<CenterClient>
    {
        public CenterClient Login { get; set; }
        public Worlds Worlds { get; private set; }
        public Maple.Migrations Migrations { get; private set; }

        public CenterServer()
        {
            Worlds = new Worlds();
            Migrations = new Maple.Migrations();
            byte[] listenIp = { 0, 0, 0, 0 };
            Start(new IPAddress(listenIp), ServerConfig.Instance.CenterPort);
        }
    }
}
