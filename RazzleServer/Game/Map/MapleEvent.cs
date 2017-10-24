using RazzleServer.Data.WZ;

namespace RazzleServer.Map
{
    public class MapleEvent : MapleMap
    {
        public MapleEvent(int mapId, WzMap wzMap, bool skipSpawn) : base(mapId, wzMap, skipSpawn) { }
    }
}
