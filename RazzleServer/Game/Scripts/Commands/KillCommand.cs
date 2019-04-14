using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class KillCommand : ACommandScript
    {
        public override string Name => "kill";

        public override string Parameters => "[ -map | -character ]";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
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
                        foreach (var character in caller.Map.Characters.Values)
                        {
                            if (character != caller && !character.IsMaster)
                            {
                                character.PrimaryStats.Health = 0;
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
                        var target = caller.Map.Characters[targetName];

                        if (target == null)
                        {
                            caller.Notify("[Command] " + targetName + " cannot be found.");
                        }
                        else
                        {
                            target.PrimaryStats.Health = 0;
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
