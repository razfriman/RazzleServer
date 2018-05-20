using System.Collections.Generic;
using System.Collections.ObjectModel;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledMobs : KeyedCollection<int, Mob>
    {
        public Character Parent { get; private set; }

        public ControlledMobs(Character parent)
        {
            Parent = parent;
        }

        protected override void InsertItem(int index, Mob item)
        {
            lock (this)
            {
                if (Parent.Client.Connected)
                {
                    item.Controller = Parent;

                    base.InsertItem(index, item);

                    Parent.Client.Send(item.GetControlRequestPacket());
                }
                else
                {
                    item.AssignController();
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                var item = Items[index];

                if (Parent.Client.Connected)
                {
                    Parent.Client.Send(item.GetControlCancelPacket());
                }

                item.Controller = null;

                base.RemoveItem(index);
            }
        }

        protected override void ClearItems()
        {
            lock (this)
            {
                var toRemove = new List<Mob>();

                foreach (var mob in this)
                {
                    toRemove.Add(mob);
                }

                foreach (var mob in toRemove)
                {
                    Remove(mob);
                }
            }
        }

        protected override int GetKeyForItem(Mob item) => item.ObjectId;
    }
}
