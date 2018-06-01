using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledNpcs : MapleKeyedCollection<int, Npc>
    {
        public Character Parent { get; }

        public ControlledNpcs(Character parent)
        {
            Parent = parent;
        }

        public override void Add(Npc item)
        {
            lock (this)
            {
                if (Parent.Client.Connected)
                {
                    item.Controller = Parent;
                    base.Add(item);
                    //this.Parent.Client.Send(item.GetControlRequestPacket());
                }
                else
                {
                    item.AssignController();
                }
            }
        }

        public override void Remove(Npc item)
        {
            lock (this)
            {
                if (Parent.Client.Connected)
                {
                        Parent.Client.Send(item.GetControlCancelPacket());
                }

                item.Controller = null;

                base.Remove(item);
            }
        }

        public override int GetKey(Npc item) => item.ObjectId;
    }
}
