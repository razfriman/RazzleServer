using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledNpcs : MapleKeyedCollection<int, Npc>
    {
        public GameCharacter Parent { get; }

        public ControlledNpcs(GameCharacter parent)
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
                    //this.Parent.Send(item.GetControlRequestPacket());
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
                    Parent.Send(item.GetControlCancelPacket());
                }

                item.Controller = null;

                base.Remove(item);
            }
        }

        public override int GetKey(Npc item) => item.ObjectId;
    }
}
