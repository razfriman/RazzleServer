using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapDrops : MapObjects<Drop>
    {
        public MapDrops(Map map) : base(map) { }

        public override void OnItemAdded(Drop item)
        {
            item.Picker = null;
            item.Expiry?.Dispose();

            item.Expiry = new Delay(() =>
            {
                if (item.Map == Map)
                {
                    Remove(item);
                }
            }, Drop.ExpiryTime);

            lock (Map.Characters)
            {
                foreach (var character in Map.Characters.Values)
                {
                    using (var oPacket = item.GetCreatePacket(item.Owner == null ? character : null))
                    {
                        character.Client.Send(oPacket);
                    }
                }
            }
        }

        public override void OnItemRemoved(Drop item)
        {
            item.Expiry?.Dispose();
            Map.Send(item.GetDestroyPacket());
        }
    }
}
