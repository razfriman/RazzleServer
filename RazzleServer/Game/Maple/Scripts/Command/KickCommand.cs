using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripts.Command{
    public sealed class KickCommand : ACommandScript
    {
        public override string Name => "kick";

        public override string Parameters => "[character]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length == 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                var name = args[0];
                var target = caller.Client.Server.World.GetCharacterByName(name);

                if (target == null)
                {
                    caller.Notify($"[Command] Character '{name}' could not be found.");
                    return;
                }

                target.Client.Terminate($"Player was kicked by {caller.Name}");
            }
        }
    }
}
