using RazzleServer.Game.Maple.Characters;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class HelpCommand : Command
    {
        public override string Name => "help";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => false;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                caller.Notify("[Help]");

                foreach (var command in CommandFactory.Commands)
                {
                    if ((command.IsRestricted && caller.IsMaster) || !command.IsRestricted && !(command is HelpCommand))
                    {
                        caller.Notify($"{ServerConfig.Instance.CommandIndicator} {command.Name} {command.Parameters}");
                    }
                }
            }
        }
    }
}
