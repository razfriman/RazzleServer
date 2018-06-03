using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class AReactorScript
    {
        public Character Character { get; set; }

        public Reactor Reactor { get; set; }

        public abstract void Execute();
    }
}
