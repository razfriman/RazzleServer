using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class HorntailCommand : ACommandScript
    {
        public override string Name => "horntail";

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
                var mob = new Mob(8810026) {Position = caller.Position};

                caller.Map.Mobs.Add(mob);
                caller.Map.Mobs.Remove(mob);
            }
        }
    }
}
