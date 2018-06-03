using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class NpcReference
    {
        public int MapleId { get; set; }
        public int StorageCost { get; set; }
        public string Script { get; set; }

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

            if (info != null)
            {
                StorageCost = info["trunkPut"]?.GetInt() ?? 0;
                Script = info["0"]?["script"]?.GetString();
            }

        }
    }
}
