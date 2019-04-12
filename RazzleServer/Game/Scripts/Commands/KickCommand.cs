using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
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
                
                if (target.Name == caller.Name)
                {
                    caller.Notify("You cannot kick yourself");
                    return;
                }

                if (target.IsMaster)
                {
                    caller.Notify("You cannot kick a GM");
                    return;
                }

                target.Client.Terminate($"Player was kicked by {caller.Name}");
            }
        }
    }
}
