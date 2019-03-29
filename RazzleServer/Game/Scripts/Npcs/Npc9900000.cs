using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("9900000")]
    public class Npc9900000 : ANpcScript
    {
        public override void Execute()
        {
            SendOk("Welcome to RazzleServer");
        }
    }
}
