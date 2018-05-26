using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Scripts
{
    public abstract class AReactorScript
    {
        public Character Character { get; set; }

        public abstract string Name { get; }

        public abstract void Action();
    }
}
