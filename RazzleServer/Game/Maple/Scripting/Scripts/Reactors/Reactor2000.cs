namespace RazzleServer.Game.Maple.Scripting.Scripts.Reactors
{
    [ReactorScript("2000")]
    public class Reactor2000 : AReactorScript
    {
        public override void Execute()
        {
            System.Console.WriteLine("Reactor Script");
        }
    }
}
