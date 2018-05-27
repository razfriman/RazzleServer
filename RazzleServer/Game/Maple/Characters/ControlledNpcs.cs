using System.Collections.Generic;
using System.Collections.ObjectModel;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledNpcs : KeyedCollection<int, Npc>
    {
        public Character Parent { get; }

        public ControlledNpcs(Character parent)
        {
            Parent = parent;
        }

        protected override void InsertItem(int index, Npc item)
        {
            lock (this)
            {
                if (Parent.Client.Connected)
                {
                    item.Controller = Parent;
                    base.InsertItem(index, item);
                    //this.Parent.Client.Send(item.GetControlRequestPacket());
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
                var toRemove = new List<Npc>();

                foreach (var npc in this)
                {
                    toRemove.Add(npc);
                }

                foreach (var npc in toRemove)
                {
                    Remove(npc);
                }
            }
        }

        protected override int GetKeyForItem(Npc item)
        {
            return item.ObjectId;
        }
    }
}
