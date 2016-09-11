using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Server
{
    public static class ServerManager
    {
        public static LoginServer LoginServer { get; set; }
        public static Dictionary<int, ChannelServer> ChannelServers { get; set; } = new Dictionary<int, ChannelServer>();
    }
}
