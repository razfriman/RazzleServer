using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class MaxCommand : ACommandScript
    {
        public override string Name => "max";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            caller.PrimaryStats.Level = 200;
            caller.PrimaryStats.Experience = 0;
            caller.PrimaryStats.MaxHealth = 30000;
            caller.PrimaryStats.Health = 30000;
            caller.PrimaryStats.MaxMana = 30000;
            caller.PrimaryStats.Mana = 30000;
            caller.PrimaryStats.Strength = 10000;
            caller.PrimaryStats.Dexterity = 10000;
            caller.PrimaryStats.Intelligence = 10000;
            caller.PrimaryStats.Luck = 10000;
            caller.PrimaryStats.Meso = int.MaxValue;
        }
    }
}
