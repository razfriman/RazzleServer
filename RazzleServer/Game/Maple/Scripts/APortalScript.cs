using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Scripts
{
    public class APortalScript : ScriptBase
    {
        private Portal _portal;

        public APortalScript(Portal portal, Character character)
            : base(ScriptType.Portal, portal.Script, character)
        {
            _portal = portal;
        }

        private void PlayPortalSoundEffect()
        {
            Character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
        }
    }
}
