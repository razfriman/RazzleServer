using RazzleServer.Wz;

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

            if (info == null)
            {
                return;
            }

            Script = info["quest"]?.GetString();
            StorageCost = info["trunk"]?.GetInt() ?? 0;
        }
    }
}
