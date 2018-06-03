using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class MesoCommand : ACommandScript
    {
        public override string Name => "meso";

        public override string Parameters => "amount";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                if (uint.TryParse(args[0], out var amount))
                {
                    caller.Meso += (int)amount;
                }
            }
        }
    }
}
