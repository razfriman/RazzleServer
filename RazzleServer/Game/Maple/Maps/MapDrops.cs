using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapDrops : MapObjects<Drop>
    {
        public MapDrops(Map map) : base(map) { }

        public override void Add(Drop item)
        {
            item.Picker = null;
            base.Add(item);
            ScheduleExpiration(item);

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

        private void ScheduleExpiration(Drop item)
        {
            item.Expiry?.Dispose();

            item.Expiry = new Delay(() =>
            {
                if (item.Map == Map)
                {
                    item.Picker = null;
                    Remove(item);
                }
            }, Drop.ExpiryTime);
        }

        public override void Remove(Drop item)
        {
            item.Expiry?.Dispose();
            Map.Send(item.GetDestroyPacket());
            base.Remove(item);
        }
    }
}
