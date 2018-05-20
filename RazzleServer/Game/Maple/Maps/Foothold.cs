using RazzleServer.Common.WzLib;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Foothold
    {
        public short Id { get; private set; }
        public Line Line { get; private set; }
        public short DragForce { get; private set; }
        public bool ForbidDownwardJump { get; private set; }

        public Foothold(WzImageProperty img)
        {
            //Id = (short)(int)img["id"];
            //Line = new Line(new Point((short)img["x1"], (short)img["y1"]), new Point((short)img["x2"], (short)img["y2"]));
            //DragForce = (short)img["drag_force"];
            //ForbidDownwardJump = ((string)img["flags"]).Contains("forbid_downward_jump");
        }
    }
}
