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
                this.ShowSyntax(caller);
            }
            else
            {
                int amount = 0;
                bool isAmountSpecified;

                if (args.Length > 1)
                {
                    isAmountSpecified = int.TryParse(args[args.Length - 1], out amount);
                }
                else
                {
                    isAmountSpecified = false;
                }

                if (amount < 1)
                {
                    amount = 1;
                }

                if (int.TryParse(args[0], out var mobId))
                {
                    if (DataProvider.Mobs.Contains(mobId))
                    {
                        for (int i = 0; i < amount; i++)
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
