using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapDrops : MapObjects<Drop>
    {
        public MapDrops(Map map) : base(map) { }

        protected override void InsertItem(int index, Drop item)
        {
            item.Picker = null;

            base.InsertItem(index, item);

            if (item.Expiry != null)
            {
                item.Expiry.Dispose();
            }

            item.Expiry = new Delay(() =>
            {
                if (item.Map == Map)
                {
                    Remove(item);
                }
            }, Drop.ExpiryTime);

            lock (Map.Characters)
            {
                foreach (var character in Map.Characters)
                {
                    using (var oPacket = item.GetCreatePacket(item.Owner == null ? character : null))
                    {
                        character.Client.Send(oPacket);
                    }
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            if (item.Expiry != null)
            {
                item.Expiry.Dispose();
            }

            Map.Broadcast(item.GetDestroyPacket());
            base.RemoveItem(index);
        }
    }
}
