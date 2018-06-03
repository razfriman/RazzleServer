namespace RazzleServer.Game.Maple.Scripting.Scripts.Npcs
{
    [NpcScript("LevelUP")]
    public class LevelUp : ANpcScript
    {
        public override void Execute()
        {
            Character.Level++;
            System.Console.WriteLine("SCRIPT - Levelup!");
        }
    }
}
