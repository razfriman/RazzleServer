using System.Collections.Generic;
using System.Collections.ObjectModel;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledNpcs : KeyedCollection<int, Npc>
    {
        public Character Parent { get; private set; }

        public ControlledNpcs(Character parent)
        {
            this.Parent = parent;
        }

        public void Move(PacketReader iPacket)
        {
            int objectID = iPacket.ReadInt();

            Npc npc;

            try
            {
                npc = this[objectID];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            npc.Move(iPacket);
        }

        protected override void InsertItem(int index, Npc item)
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
                Npc item = base.Items[index];

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
                List<Npc> toRemove = new List<Npc>();

                foreach (Npc npc in this)
                {
                    toRemove.Add(npc);
                }

                foreach (Npc npc in toRemove)
                {
                    this.Remove(npc);
                }
            }
        }

        protected override int GetKeyForItem(Npc item)
        {
            return item.ObjectID;
        }
    }
}
