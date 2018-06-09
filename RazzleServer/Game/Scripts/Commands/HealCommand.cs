using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class HealCommand : ACommandScript
    {
        public override string Name => "heal";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            caller.Health = caller.MaxHealth;
            caller.Mana = caller.MaxMana;
        }
    }
}
