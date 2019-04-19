using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Server;
using RazzleServer.Login.Maple;
using Serilog;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient, LoginPacketHandler>, ILoginServer
    {
        public LoginServer(IServerManager manager) : base(manager) => Port = ServerConfig.Instance.LoginPort;

        public override ILogger Logger => Log.ForContext<LoginServer>();

        public bool CharacterExists(string name, byte world)
        {
            using var dbContext = new MapleDbContext();
            return dbContext.Characters
                .Where(x => x.WorldId == world)
                .Any(x => x.Name == name);
        }

        public List<Character> GetCharacters(byte worldId, int accountId)
        {
            using var dbContext = new MapleDbContext();
            var result = new List<Character>();

            var characters = dbContext
                .Characters
                .Where(x => x.AccountId == accountId)
                .Where(x => x.WorldId == worldId);

            characters
                .ToList()
                .ForEach(x =>
                {
                    var c = new LoginCharacter {Id = x.Id};
                    c.Load();
                    result.Add(c);
                });

            return result;
        }
    }
}
