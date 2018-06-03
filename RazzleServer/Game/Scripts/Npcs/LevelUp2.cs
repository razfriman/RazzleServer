namespace RazzleServer.Game.Maple.Scripting.Scripts.Npcs
{
    [NpcScript("levelUP2")]
    public class LevelUp2 : ANpcScript
    {
        public override void Execute()
        {
            SendOk("Welcome to RazzleServer.");
            SendNext("Next Page");
            SendBackNext("More page");
            SendBackNext("Last page");
            System.Console.WriteLine("SCRIPT - Levelup2!");
        }
    }
}