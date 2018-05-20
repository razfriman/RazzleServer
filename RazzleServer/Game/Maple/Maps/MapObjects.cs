using System.Collections.Generic;
using System.Collections.ObjectModel;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObjects<T> : KeyedCollection<int, T> where T : MapObject
    {
        public Map Map { get; private set; }

        public MapObjects(Map map)
        {
            Map = map;
        }

        public IEnumerable<T> GetInRange(MapObject reference, int range)
        {
            foreach (var loopObject in this)
            {
                if (reference.Position.DistanceFrom(loopObject.Position) <= range)
                {
                    yield return loopObject;
                }
            }
        }

        protected override int GetKeyForItem(T item)
        {
            return item.ObjectId;
        }

        protected override void InsertItem(int index, T item)
        {
            item.Map = Map;

            if (!(item is Character)  && !(item is Portal))
            {
                item.ObjectId = Map.AssignObjectId();
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            item.Map = null;

            if (!(item is Character) && !(item is Portal))
            {
                item.ObjectId = -1;
            }

            base.RemoveItem(index);
        }
    }
}
