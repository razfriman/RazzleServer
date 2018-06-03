using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("levelUP")]
    public class LevelUp : ANpcScript
    {
        public override void Execute()
        {
            Character.Level++;
        }
    }
}
