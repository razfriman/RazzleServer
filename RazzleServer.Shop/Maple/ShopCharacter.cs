using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Net.Packet;
using RazzleServer.Server;
using Serilog;

namespace RazzleServer.Shop.Maple
{
    public class ShopCharacter : ICharacter
    {
        private readonly ILogger _log = Log.ForContext<ShopCharacter>();

        public ShopClient Client { get; set; }
        public int Id { get; set; }
        public int AccountId { get; set; }
        public bool IsMaster => Client.Account.IsMaster;
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public int MapId { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }


        public ShopCharacter(int id = 0, ShopClient client = null)
        {
            Id = id;
            Client = client;
        }

        public void Initialize()
        {
            using var pw = new PacketWriter(ServerOperationCode.SetFieldCashShop);
            Client.Send(pw);
            Client.StartPingCheck();
        }

        public void LogCheatWarning(CheatType type)
        {
            using var dbContext = new MapleDbContext();
            _log.Information($"Cheat Warning: Character={Id} CheatType={type}");
            dbContext.Cheats.Add(new CheatEntity {CharacterId = Id, CheatType = (int)type});
            dbContext.SaveChanges();
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
            var character = dbContext.Characters.Find(Id);

            if (character == null)
            {
                _log.Error($"Cannot find character [{Id}]");
                return;
            }

            Name = character.Name;
            AccountId = character.AccountId;
            MapId = character.MapId;
            SpawnPoint = character.SpawnPoint;
            WorldId = character.WorldId;
        }

        public void Send(PacketWriter packet) => Client.Send(packet);
    }
}
