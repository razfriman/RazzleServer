using System.Collections.Generic;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    public class NpcReference
    {
        private readonly ILogger _log = Log.ForContext<NpcReference>();

        public int MapleId { get; set; }
        public int StorageCost { get; set; }

        public string Script { get; set; }
        public byte SpeakLineCount { get; set; }

        public short Speed { get; set; }
        public readonly List<NpcShopItemReference> ShopItems = new List<NpcShopItemReference>();
        public readonly Dictionary<string, int> Variables = new Dictionary<string, int>();
        public NpcReference() { }

        public NpcReference(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
            var info = img["info"];

            foreach (var node in info.WzProperties)
            {
                switch (node.Name)
                {
                    case "hideName":
                    case "link":
                    case "float": // Floating NPC
                    case "default": // Icon used for npc chat dialog, see NPC 2030006
                    case "dcTop": // Double Click mark
                    case "dcRight":
                    case "dcBottom":
                    case "dcLeft":
                    case "dcMark":
                        break;
                    case "reg":
                        node.WzProperties.ForEach(x => Variables[x.Name] = x.GetInt());
                        break;
                    case "quest":
                        Script = node.GetString();
                        break;
                    case "trunk":
                        StorageCost = node.GetInt();
                        break;
                    case "speed":
                        Speed = node.GetShort();
                        break;
                    case "speak":
                        SpeakLineCount = (byte)node.WzProperties.Count;
                        break;
                    case "shop":
                        node.WzProperties.ForEach(x => ShopItems.Add(new NpcShopItemReference(x)));
                        break;
                    default:
                        _log.Warning($"Unknown npc info node Npc={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
