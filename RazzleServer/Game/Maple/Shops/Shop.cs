using System.Collections.Generic;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Shops
{
    public sealed class Shop
    {
        private byte RechargeTierId { get; set; }
        public int Id { get; private set; }
        public Npc Parent { get; private set; }
        public List<ShopItem> Items { get; private set; }
        public Dictionary<int, double> UnitPrices => RechargeTiers[RechargeTierId];
        public static Dictionary<byte, Dictionary<int, double>> RechargeTiers { get; set; }

        public static void LoadRechargeTiers()
        {
            RechargeTiers = new Dictionary<byte, Dictionary<int, double>>();

            //foreach (Datum datum in new Datums("shop_recharge_data").Populate())
            //{
            //    if (!RechargeTiers.ContainsKey((byte)(int)datum["tierid"]))
            //    {
            //        RechargeTiers.Add((byte)(int)datum["tierid"], new Dictionary<int, double>());
            //    }

            //    RechargeTiers[(byte)(int)datum["tierid"]].Add((int)datum["itemid"], (double)datum["price"]);
            //}
        }

        //public Shop(Npc parent, Datum datum)
        //{
        //    Parent = parent;

        //    Id = (int)datum["shopid"];
        //    RechargeTierId = (byte)(int)datum["recharge_tier"];

        //    Items = new List<ShopItem>();

        //    foreach (Datum itemDatum in new Datums("shop_items").Populate("shopid = {0} ORDER BY sort DESC", Id))
        //    {
        //        Items.Add(new ShopItem(this, itemDatum));
        //    }

        //    if (RechargeTierId > 0)
        //    {
        //        foreach (KeyValuePair<int, double> rechargeable in UnitPrices)
        //        {
        //            Items.Add(new ShopItem(this, rechargeable.Key));
        //        }
        //    }
        //}

        public void Show(Character customer)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.OpenNpcShop))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteShort((short)Items.Count);

                foreach (ShopItem loopShopItem in Items)
                {
                    oPacket.WriteBytes(loopShopItem.ToByteArray());
                }

                customer.Client.Send(oPacket);
            }
        }
    }
}
