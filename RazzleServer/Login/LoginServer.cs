using System.Collections.Generic;
using System.Linq;
using System.Net;
using RazzleServer.Common;
using RazzleServer.Data;
using RazzleServer.Login.Maple;
using Serilog;

namespace RazzleServer.Login
{
    public class LoginServer : MapleServer<LoginClient, LoginPacketHandler>
    {
        public LoginServer(AServerManager manager) : base(manager)
        {
            Port = ServerConfig.Instance.LoginPort;
        }

        public void Start() => Start(new IPAddress(new byte[] {0, 0, 0, 0}), Port);

        public override ILogger Logger => Log.ForContext<LoginServer>();

        internal bool CharacterExists(string name, byte world)
        {
            using var dbContext = new MapleDbContext();
            return dbContext.Characters
                .Where(x => x.WorldId == world)
                .Any(x => x.Name == name);
        }

        internal List<LoginCharacter> GetCharacters(byte worldId, int accountId)
        {
            using var dbContext = new MapleDbContext();
            var result = new List<LoginCharacter>();

            var characters = dbContext
                .Characters
                .Where(x => x.AccountId == accountId)
                .Where(x => x.WorldId == worldId);

            characters
                .ToList()
                .ForEach(x =>
                {
                    var c = new LoginCharacter() {Id = x.Id};
                    c.Load();
                    result.Add(c);
                });

            return result;
        }
    }
}
