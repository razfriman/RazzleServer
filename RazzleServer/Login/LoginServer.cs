using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using RazzleServer.Center;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Server;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient, LoginPacketHandler>
    {
        public LoginServer(ServerManager manager) : base(manager)
        {
            Port = ServerConfig.Instance.LoginPort;
            Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);
        }

        internal List<Character> GetCharacters(byte worldId, int accountId)
        {
            using (var dbContext = new MapleDbContext())
            {
                var result = new List<Character>();

                var characters = dbContext
                    .Characters
                    .Where(x => x.AccountId == accountId)
                    .Where(x => x.WorldId == worldId);

                characters.ToList()
                          .ForEach(x =>
                          {
                              var c = new Character
                              {
                                  Id = x.Id
                              };
                              c.Load();
                              result.Add(c);
                          });

                return result;
            }
        }
    }
}