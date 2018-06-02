using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Scripts
{
    public abstract class APortalScript
    {
        public Character Character { get; }
        public Portal Portal { get; }

        public abstract string Name { get; }

        public abstract void Execute();

        private void PlayPortalSoundEffect()
        {
            Character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
        }
    }
}
