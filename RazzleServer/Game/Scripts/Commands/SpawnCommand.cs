using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class SpawnCommand : ACommandScript
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
                if (args.Length < 2 || !int.TryParse(args[1], out var amount))
                {
                    amount = 1;
                }

                if (amount < 1)
                {
                    amount = 1;
                }

                int.TryParse(args[0], out var mobId);
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
