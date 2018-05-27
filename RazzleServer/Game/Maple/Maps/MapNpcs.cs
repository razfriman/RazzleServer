using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapNpcs : MapObjects<Npc>
    {
        public MapNpcs(Map map) : base(map) { }

        public override void OnItemAdded(Npc item)
        {
            Map.Send(item.GetCreatePacket());
            item.AssignController();
        }

        public override void OnItemRemoved(Npc item)
        {
            item.Controller.ControlledNpcs.Remove(item);
            Map.Send(item.GetDestroyPacket());
        }
    }
}
