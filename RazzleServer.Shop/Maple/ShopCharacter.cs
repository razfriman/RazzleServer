using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Maple;
using RazzleServer.Data;
using RazzleServer.Net.Packet;
using RazzleServer.Server.Maple;
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
        public BasicCharacterStats PrimaryStats { get; set; }
        public BasicCharacterItems Items { get; set; }


        public ShopCharacter(int id = 0, ShopClient client = null)
        {
            Id = id;
            Client = client;
            PrimaryStats = new BasicCharacterStats(this);
        }

        public void Initialize()
        {
            SendEnterField();
            SendCashAmounts();
            SendWishList();
            SendLocker();
            SendGifts();
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
            Items = new BasicCharacterItems(this, character.EquipmentSlots, character.UsableSlots, character.SetupSlots,
                character.EtceteraSlots, character.CashSlots);
            Items.Load();
        }

        public void Send(PacketWriter packet) => Client.Send(packet);

        public void SendEnterField()
        {
            using var pw = new PacketWriter(ServerOperationCode.SetFieldCashShop);

            var flags = CharacterDataFlags.All;
            //var flags = CharacterDataFlags.CashShop;
            pw.WriteShort((ushort)flags);

            if (flags.HasFlag(CharacterDataFlags.Stats))
            {
                pw.WriteBytes(PrimaryStats.ToByteArray());
                pw.WriteByte(20); // Buddylist slots
            }

            if (flags.HasFlag(CharacterDataFlags.Money))
            {
                pw.WriteInt(PrimaryStats.Meso);
            }

            pw.WriteBytes(Items.ToByteArray(flags));

            if (flags.HasFlag(CharacterDataFlags.Skills))
            {
                pw.WriteShort(0);
//                pw.WriteShort((short)Skills.Count);
//                foreach (var skillId in chr.Skills)
//                {
//                    pw.WriteInt(skillId);
//                    pw.WriteInt(1);
//                }
            }

            pw.WriteShort(0); // minigame
            pw.WriteShort(0); // rings

            for (var i = 0; i < 5; i++)
            {
                pw.WriteInt(999999999);
            }


            pw.WriteBool(true);
            pw.WriteString(Name);


            pw.WriteShort(0);
            //pack.WriteShort((short)itemsNotOnSale.Count);
            //itemsNotOnSale.ForEach(pack.WriteInt);


            // BEST
            // Categories
            for (byte i = 1; i <= 8; i++)
            {
                // Gender (0 = male, 1 = female)
                for (byte j = 0; j <= 1; j++)
                {
                    // Top 5 items
                    for (byte k = 0; k < 5; k++)
                    {
                        pw.WriteInt(i);
                        pw.WriteInt(j);
                        pw.WriteInt(0);

//                        if (Server.Instance.BestItems.TryGetValue((i, j, k), out var sn))
//                        {
//                            pack.WriteInt(sn);
//                        }
                    }
                }
            }

            // -1 == available, 2 is not available, 1 = default?

//            var customStockState = DataProvider.Commodity.Values.Where(x => x.StockState != StockState.DefaultState)
//                .ToList();
            var customStockState = new List<(int serialNumber, int stockState)>();
            pw.WriteShort(customStockState.Count);
            customStockState.ForEach(x =>
            {
                pw.WriteInt(x.serialNumber);
                pw.WriteInt(x.stockState);
            });

            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            Send(pw);
        }

        private void SendCashAmounts()
        {
            using var pw = new PacketWriter(ServerOperationCode.CashShopAmounts);
            pw.WriteInt(0);
            pw.WriteInt(0);
            Send(pw);
        }

        private void SendWishList(bool isUpdate = false)
        {
            using var pw = new PacketWriter(ServerOperationCode.CashShopOperation);
            pw.WriteByte(isUpdate ? CashShopAction.ServerUpdateWishList : CashShopAction.ServerLoadWishList);
            for (int i = 0; i < 10; i++)
            {
                pw.WriteInt(0);
            }

            Send(pw);
        }

        private void SendLocker()
        {
            using var pw = new PacketWriter(ServerOperationCode.CashShopOperation);
            pw.WriteByte(CashShopAction.ServerLoadLocker);
            pw.WriteByte(0); // Encode item and set unread = false
            pw.WriteShort(3); // Storage slots
            Send(pw);
        }

        private void SendGifts()
        {
        }
    }
}
