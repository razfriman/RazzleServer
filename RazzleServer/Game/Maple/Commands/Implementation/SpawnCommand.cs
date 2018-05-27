using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class SpawnCommand : Command
    {
        public override string Name => "spawn";

        public override string Parameters => "{ id } [ amount ] ";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                var amount = 0;
                var isAmountSpecified = args.Length > 1 && int.TryParse(args[args.Length - 1], out amount);

                if (amount < 1)
                {
                    amount = 1;
                }

                if (int.TryParse(args[0], out var mobId))
                {
                    if (DataProvider.Mobs.Data.ContainsKey(mobId))
                    {
                        for (var i = 0; i < amount; i++)
                        {
                            caller.Map.Mobs.Add(new Mob(mobId, caller.Position));
                        }
                    }
                    else
                    {
                        caller.Notify("[Command] Invalid mob.");
                    }
                }
            }
        }
    }
}
