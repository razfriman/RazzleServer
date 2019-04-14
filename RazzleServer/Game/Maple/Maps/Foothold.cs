using RazzleServer.Common.Util;
using RazzleServer.DataProvider.References;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Foothold
    {
        public short Id { get; }
        public Line Line { get; set; }
        public short DragForce { get; }
        public bool ForbidDownwardJump { get; }
        public bool IsWall => Line.Start.X == Line.End.X;

        public Foothold() { }

        public Foothold(FootholdReference reference)
        {
            Id = reference.Id;
            Line = reference.Line;
            DragForce = reference.DragForce;
            ForbidDownwardJump = reference.ForbidDownwardJump;
        }
    }
}
