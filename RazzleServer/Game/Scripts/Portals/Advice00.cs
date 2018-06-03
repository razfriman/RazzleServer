using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Portals
{
    [PortalScript("advice00")]
    public class Advice00 : APortalScript
    {
        public override void Execute() => ShowInstruction("You can move by using the arrow keys.");
    }
}
