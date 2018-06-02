using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class NpcReference
    {
        public int MapleId { get; set; }
        public int StorageCost { get; set; } //trunkPut

        public NpcReference(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
            StorageCost = img["trunkPut"]?.GetInt() ?? 0;
        }
    }
}
