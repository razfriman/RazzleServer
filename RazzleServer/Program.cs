using System.Collections.Generic;
using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Data;
using System;
using System.Linq;

namespace RazzleServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MapleClient.RegisterPacketHandlers();

            Console.WriteLine("Initializing Database");
            using (var context = new MapleDbContext())
            {
                var acc = MapleAccount.GetAccountFromDatabase("admin");
                var accounts = context.Accounts.ToArray();
                Console.WriteLine($"Accounts: {context.Accounts.Count()}");
            }

            ServerManager.LoginServer = new LoginServer();

            for (var i = 0; i < ServerConfig.Instance.Channels; i++)
            {
                var channelServer = new ChannelServer((ushort)(ServerConfig.Instance.ChannelStartPort + i));
                ServerManager.ChannelServers[i] = channelServer;
            }

            while (true)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
