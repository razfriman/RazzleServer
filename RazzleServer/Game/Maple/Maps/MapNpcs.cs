using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapNpcs : MapObjects<Npc>
    {
        public MapNpcs(Map map) : base(map) { }

        protected override void InsertItem(int index, Npc item)
        {
            base.InsertItem(index, item);

            if (DataProvider.IsInitialized)
            {
                Map.Broadcast(item.GetCreatePacket());
                item.AssignController();
            }
        }

        protected override void RemoveItem(int index)
        {
            if (DataProvider.IsInitialized)
            {
                var item = Items[index];

                item.Controller.ControlledNpcs.Remove(index);
                    Map.Broadcast(item.GetDestroyPacket());
            }

            base.RemoveItem(index);
        }
    }
}
