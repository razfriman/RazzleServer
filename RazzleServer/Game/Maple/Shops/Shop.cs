using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Packet;
using RazzleServer.Data;
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

            //foreach (var  datum in new Datums("shop_recharge_data").Populate())
            //{
            //    if (!RechargeTiers.ContainsKey((byte)(int)datum["tierid"]))
            //    {
            //        RechargeTiers.Add((byte)(int)datum["tierid"], new Dictionary<int, double>());
            //    }

            //    RechargeTiers[(byte)(int)datum["tierid"]].Add((int)datum["itemid"], (double)datum["price"]);
            //}
        }

        public Shop(Npc parent, ShopEntity entity)
        {
            Parent = parent;

            Id = entity.Id;
            RechargeTierId = entity.RechargeTier;

            Items = new List<ShopItem>();

            foreach (var item in entity.ShopItems.OrderBy(x => x.Sort))
            {
                Items.Add(new ShopItem(this, item));
            }

            if (RechargeTierId > 0)
            {
                foreach (KeyValuePair<int, double> rechargeable in UnitPrices)
                {
                    Items.Add(new ShopItem(this, rechargeable.Key));
                }
            }
        }

        public void Show(Character customer)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.OpenNpcShop))
            {
                oPacket.WriteInt(Id);
                oPacket.WriteShort((short)Items.Count);

                foreach (var loopShopItem in Items)
                {
                    oPacket.WriteBytes(loopShopItem.ToByteArray());
                }

                customer.Client.Send(oPacket);
            }
        }
    }
}
