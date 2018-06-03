using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class APortalScript
    {
        public Character Character { get; set; }

        public Portal Portal { get; set; }

        public abstract void Execute();

        protected void PlayPortalSoundEffect() => Character.ShowLocalUserEffect(UserEffect.PlayPortalSe);

        protected void ShowInstruction(string text, int width = 0, int height = 0) => Character.SendHint(text, width, height);
    }
}
