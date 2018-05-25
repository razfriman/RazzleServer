using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class KillCommand : Command
    {
        public override string Name => "kill";

        public override string Parameters => "[ -map | -character ]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                switch (args[0])
                {
                    case "-map":
                        {
                            foreach (var character in caller.Map.Characters)
                            {
                                if (character != caller && !character.IsMaster)
                                {
                                    character.Health = 0;
                                }
                            }
                        }
                        break;

                    case "-character":
                        {
                            if (args.Length == 1)
                            {
                                ShowSyntax(caller);

                                return;
                            }

                            var targetName = args[1];

                            Character target;

                            target = caller.Map.Characters[targetName];

                            if(target == null) {
                                caller.Notify("[Command] " + targetName + " cannot be found.");
                            } else {
                                target.Health = 0;

                            }
                        }
                        break;

                    default:
                        ShowSyntax(caller);
                        break;
                }
            }
        }
    }
}
