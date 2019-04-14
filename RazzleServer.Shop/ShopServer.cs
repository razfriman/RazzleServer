using System.Collections.Generic;
using System.Linq;
using System.Net;
using RazzleServer.Common;
using RazzleServer.Common.Server;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Server;
using Serilog;

namespace RazzleServer.Shop
{
    public class ShopServer : MapleServer<ShopClient, ShopPacketHandler>, IShopServer
    {
        public ShopServer(IServerManager manager) : base(manager) => Port = ServerConfig.Instance.ShopPort;

        public void Start() => Start(new IPAddress(new byte[] {0, 0, 0, 0}), Port);

        public override ILogger Logger => Log.ForContext<ShopServer>();

        internal bool CharacterExists(string name, byte world)
        {
            using var dbContext = new MapleDbContext();
            return dbContext.Characters
                .Where(x => x.WorldId == world)
                .Any(x => x.Name == name);
        }

        internal List<GameCharacter> GetCharacters(byte worldId, int accountId)
        {
            using var dbContext = new MapleDbContext();
            var result = new List<GameCharacter>();

            var characters = dbContext
                .Characters
                .Where(x => x.AccountId == accountId)
                .Where(x => x.WorldId == worldId);

            characters
                .ToList()
                .ForEach(x =>
                {
                    var c = new GameCharacter {Id = x.Id};
                    c.Load();
                    result.Add(c);
                });

            return result;
        }
    }
}
