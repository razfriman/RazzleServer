using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class HorntailCommand : Command
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
                var mob = new Mob(8810026) // TODO: Get from strings 'Summon Horntail' so we don't have to deal with
                {
                    Position = caller.Position
                };

                caller.Map.Mobs.Add(mob);
                caller.Map.Mobs.Remove(mob);
            }
        }
    }
}
