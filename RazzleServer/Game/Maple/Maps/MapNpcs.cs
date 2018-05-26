using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapNpcs : MapObjects<Npc>
    {
        public MapNpcs(Map map) : base(map) { }

        public override void OnItemAdded(Npc item)
        {
            if (DataProvider.IsInitialized)
            {
                Map.Broadcast(item.GetCreatePacket());
                item.AssignController();
            }
        }

        public override void OnItemRemoved(Npc item)
        {
            if (DataProvider.IsInitialized)
            {
                item.Controller.ControlledNpcs.Remove(item);
                Map.Broadcast(item.GetDestroyPacket());
            }
        }
    }
}
