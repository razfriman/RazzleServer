using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class ZakumCommand : ACommandScript
    {
        public override string Name => "zakum";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                caller.Map.Mobs.Add(new Mob(8800000, caller.Position));

                for (var i = 0; i < 7; i++)
                {
                    caller.Map.Mobs.Add(new Mob(8800003 + i, caller.Position));
                }
            }
        }
    }
}
