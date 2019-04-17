using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Server;
using RazzleServer.Net.Packet;

namespace RazzleServer.Shop.Maple
{
    public class ShopCharacter : Character
    {
        private ShopClient Client { get; set; }
        public override AMapleClient BaseClient => Client;

        public ShopCharacter()
        {
        }

        public ShopCharacter(int id, ShopClient client) : base(id) => Client = client;

        public override void Initialize()
        {
            SendEnterField();
            SendCashAmounts();
            SendWishList();
            SendLocker();
            SendGifts();
            Client.StartPingCheck();
        }

        public void SendEnterField()
        {
            using var pw = new PacketWriter(ServerOperationCode.SetFieldCashShop);

            var flags = CharacterDataFlags.All;
            //var flags = CharacterDataFlags.CashShop;
            pw.WriteShort((ushort)flags);

            if (flags.HasFlag(CharacterDataFlags.Stats))
            {
                pw.WriteBytes(StatisticsToByteArray());
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
            for (var i = 0; i < 10; i++)
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
