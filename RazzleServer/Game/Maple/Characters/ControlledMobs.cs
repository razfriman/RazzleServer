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
            this.Parent = parent;
        }

        public void Move(PacketReader iPacket)
        {
            int objectID = iPacket.ReadInt();
            var mob = this[objectID];
            mob?.Move(iPacket);
        }

        protected override void InsertItem(int index, Mob item)
        {
            lock (this)
            {
                if (this.Parent.Client.Connected)
                {
                    item.Controller = this.Parent;

                    base.InsertItem(index, item);

                    this.Parent.Client.Send(item.GetControlRequestPacket());
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
                Mob item = base.Items[index];

                if (this.Parent.Client.Connected)
                {
                    this.Parent.Client.Send(item.GetControlCancelPacket());
                }

                item.Controller = null;

                base.RemoveItem(index);
            }
        }

        protected override void ClearItems()
        {
            lock (this)
            {
                List<Mob> toRemove = new List<Mob>();

                foreach (Mob mob in this)
                {
                    toRemove.Add(mob);
                }

                foreach (Mob mob in toRemove)
                {
                    this.Remove(mob);
                }
            }
        }

        protected override int GetKeyForItem(Mob item)
        {
            return item.ObjectID;
        }
    }
}
