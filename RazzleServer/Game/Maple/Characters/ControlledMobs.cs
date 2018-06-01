using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public class ControlledMobs : MapleKeyedCollection<int, Mob>
    {
        public Character Parent { get; }

        public ControlledMobs(Character parent)
        {
            Parent = parent;
        }

        public override void Add(Mob item)
        {
            lock (this)
            {
                if (Parent.Client.Connected)
                {
                    item.Controller = Parent;

                    base.Add(item);

                    Parent.Client.Send(item.GetControlRequestPacket());
                }
                else
                {
                    item.AssignController();
                }
            }
        }

        public override void Remove(Mob item)
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

        public override int GetKey(Mob item) => item.ObjectId;
    }
}
