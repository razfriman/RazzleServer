using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripts.Command{
    public sealed class LevelCommand : ACommandScript
    {
        public override string Name => "level";

        public override string Parameters => "level";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                caller.Level = byte.Parse(args[0]);
            }
        }
    }
}
