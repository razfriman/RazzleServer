using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Scripting
{
    public abstract class AReactorScript
    {
        public Character Character { get; }
        public Reactor Reactor { get; }

        public abstract string Name { get; }

        public abstract void Execute();
    }
}
