using System.Collections.Generic;
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
                    using (var pw = item.GetCreatePacket(item.Owner == null ? character : null))
                    {
                        character.Client.Send(pw);
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

        public void SpawnDrops(List<Drop> drops, Point origin)
        {
            var isRight = true;
            var offset = 25;
            var expansionCount = 0;
            var leftX = origin.X;
            var rightX = origin.X;
            var currentY = origin.Y;
            var foundWallLeft = false;
            var foundWallRight = false;
            foreach (var drop in drops)
            {
                if (isRight && !foundWallRight)
                {
                    rightX += (short)(offset * expansionCount);
                }

                if (!isRight && !foundWallLeft)
                {
                    leftX -= (short)(offset * expansionCount);
                }

                var currentX = isRight ? rightX : leftX;

                if (Map.Footholds.HasWallBetween(origin, new Point(currentX, currentY)))
                {
                    if (isRight)
                    {
                        foundWallRight = true;
                    }
                    else
                    {
                        foundWallLeft = true;
                    }
                }

                drop.Position = Map.Footholds.FindFloor(new Point(currentX, currentY));
                Map.Drops.Add(drop);

                if (isRight)
                {
                    expansionCount++;
                }

                isRight = !isRight;
            }
        }
    }
}
