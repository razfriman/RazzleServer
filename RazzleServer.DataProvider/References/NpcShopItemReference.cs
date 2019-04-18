using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    public class NpcShopItemReference
    {
        private readonly ILogger _log = Log.ForContext<NpcShopItemReference>();

        public int MapleId { get; set; }
        public byte Period { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public float UnitRechargeRate { get; set; }

        public NpcShopItemReference()
        {
        }

        public NpcShopItemReference(WzImageProperty shopItemNode)
        {
            MapleId = int.Parse(shopItemNode.Name);

            foreach (var node in shopItemNode.WzPropertiesList)
            {
                switch (node.Name)
                {
                    case "period":
                        Period = (byte)node.GetInt();
                        break;
                    case "price":
                        Price = node.GetInt();
                        break;
                    case "stock":
                        Stock = node.GetInt();
                        break;
                    case "unitPrice":
                        UnitRechargeRate = node.GetFloat();
                        break;
                    default:
                        _log.Warning(
                            $"Unknown npc shop item node Item={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
