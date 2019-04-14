using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class HealCommand : ACommandScript
    {
        public override string Name => "heal";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            caller.PrimaryStats.Health = caller.PrimaryStats.MaxHealth;
            caller.PrimaryStats.Mana = caller.PrimaryStats.MaxMana;
        }
    }
}
